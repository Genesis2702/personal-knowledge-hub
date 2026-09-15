using System.Text;
using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Storage.Validators;

public class FileValidator : IFileValidator
{
    private readonly FileUploadOptions _options;
    private const int MaxNameLength = 200;

    public FileValidator(IOptions<FileUploadOptions> options)
    {
        _options = options.Value;
    }

    public void ValidateFileName(string fileName)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (fileName.Length > MaxNameLength)
        {
            throw new ArgumentException("The file name is too long");
        }
    }

    public void ValidateFileSignature(ReadOnlySpan<byte> fileBytes, FileTypeDescriptor descriptor)
    {
        int signatureOffset = descriptor.SignatureOffset;
        byte[] signature = descriptor.Signature;
        int signatureLength = signature.Length;
        int brandBytesNumber = descriptor.BrandBytesNumber;
        
        if (fileBytes.Length < signatureOffset + brandBytesNumber + signatureLength)
        {
            throw new UnsupportedMediaTypeException("This file format is not supported");
        }

        if (!fileBytes.Slice(signatureOffset, signatureLength).SequenceEqual(signature))
        {
            throw new UnsupportedMediaTypeException("This file format is not supported");
        }

        if (descriptor.Extension == "mp4")
        {
            ReadOnlySpan<byte> brandBytes = fileBytes.Slice(8, 4);
            string brand = Encoding.ASCII.GetString(brandBytes);
            if (!descriptor.AllowedBrands.Contains(brand))
            {
                throw new UnsupportedMediaTypeException("This file format is not supported");
            }
        }
    }

    public void ValidateFileSize(long fileSize)
    {
        if (fileSize > _options.MaxStoredFileSizeInBytes)
        {
            throw new FileSizeLimitExceededException("The requested file is too large");
        }
    }
}