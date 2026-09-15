using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;

namespace PersonalKnowledgeHub.Storage.Validators;

public static class FileTypeRegistry
{
    private static readonly Dictionary<string, FileTypeDescriptor> FileTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["pdf"] = new FileTypeDescriptor
            {
                Extension = "pdf",
                ContentType = "application/pdf",
                FileFormat = FileFormat.Pdf,
                Signature = new byte[]
                {
                    0x25, 0x50, 0x44, 0x46, 0x2d
                },
                SignatureOffset = 0
            },
            ["png"] = new FileTypeDescriptor
            {
                Extension = "png",
                ContentType = "image/png",
                FileFormat = FileFormat.Png,
                Signature = new byte[]
                {
                    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
                },
                SignatureOffset = 0
            },
            ["mp4"] = new FileTypeDescriptor
            {
                Extension = "mp4",
                ContentType = "video/mp4",
                FileFormat = FileFormat.Mp4,
                Signature = new byte[]
                {
                    0x66, 0x74, 0x79, 0x70
                },
                SignatureOffset = 4,
                AllowedBrands = new HashSet<string>
                {
                    "isom",
                    "mp41",
                    "mp42",
                    "avc1"
                },
                BrandBytesNumber = 4
            }
        };

    public static bool TryGet(string extension, out FileTypeDescriptor? descriptor)
    {
        string normalizedExtension = extension.TrimStart('.').ToLowerInvariant();
        return FileTypes.TryGetValue(normalizedExtension, out descriptor);
    }

    public static FileTypeDescriptor GetRequired(string extension)
    {
        string normalizedExtension = extension.TrimStart('.').ToLowerInvariant();
        if (!TryGet(normalizedExtension, out FileTypeDescriptor? descriptor))
        {
            throw new UnsupportedMediaTypeException("This file format is not supported");
        }

        return descriptor!;
    }
}