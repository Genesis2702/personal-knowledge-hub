using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Storage.Validators;

public class FileProcessor : IFileProcessor
{
    private readonly IFileValidator _validator;

    public FileProcessor(IFileValidator validator)
    {
        _validator = validator;
    }
    
    public async Task<ValidatedFile> ValidateAndStageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken)
    {
        _validator.ValidateFileName(fileName);
        
        var stagedContent = new MemoryStream();
        FileTypeDescriptor descriptor = FileTypeRegistry.GetRequired(Path.GetExtension(fileName));
        
        try
        {
            byte[] buffer = new byte[8192];
            long totalBytesRead = 0;

            while (true)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);

                if (bytesRead == 0) break;

                totalBytesRead += bytesRead;
                _validator.ValidateFileSize(totalBytesRead);

                await stagedContent.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }
            
            _validator.ValidateFileSignature(stagedContent.GetBuffer().AsSpan(0, checked((int)stagedContent.Length)), descriptor);
            stagedContent.Position = 0;

            return new ValidatedFile
            {
                Content = stagedContent,
                SizeInBytes = totalBytesRead,
                Extension = descriptor.Extension,
                ContentType = descriptor.ContentType,
                FileFormat = descriptor.FileFormat
            };

        }
        catch
        {
            await stagedContent.DisposeAsync();
            throw;
        }
    }
}