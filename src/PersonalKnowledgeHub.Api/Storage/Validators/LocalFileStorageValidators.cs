using System.Text;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Storage.Validators;

public static class LocalFileStorageValidators
{
    private static readonly Dictionary<string, (byte[] Signature, int Offset)> FileSignature = new Dictionary<string, (byte[], int)>()
    {
        { "pdf", (new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2d }, 0) },
        { "png", (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0) },
        { "mp4", (new byte[] { 0x66, 0x74, 0x79, 0x70 }, 4) },
    };
    
    private static readonly HashSet<string> AllowedMp4Brands =
    [
        "isom",
        "mp41",
        "mp42",
        "avc1"
    ];
    
    public static bool IsStoredKeyValid(string storedKey, int userId, HashSet<string> allowedExtensions)
    {
        if (String.IsNullOrEmpty(storedKey)) return false;
        
        char[] separators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        string[] segments = storedKey.Split(separators);

        if (segments.Length != 4) return false;
        
        string userIdSegment = segments[0];
        string yearSegment = segments[1];
        string monthSegment = segments[2];
        
        string[] fileSegments = segments[3].Split('.');

        if (fileSegments.Length != 2) return false;

        string fileName = fileSegments[0];
        string fileExtension = fileSegments[1];

        if (Int32.TryParse(userIdSegment, out int id))
        {
            if (id != userId) return false;
        }
        else return false;

        if (Int32.TryParse(yearSegment, out int year))
        {
            if (year > DateTime.UtcNow.Year) return false;
        }
        else return false;

        if (Int32.TryParse(monthSegment, out int month))
        {
            if (year < DateTime.UtcNow.Year && (month < 1 || month > 12)) return false;
            if (year == DateTime.UtcNow.Year && (month > DateTime.UtcNow.Month || month < 1)) return false;
        }
        else return false;
        
        if (!Guid.TryParse(fileName, out Guid guid)) return false;

        fileExtension = fileExtension.ToLower();
        if (!allowedExtensions.Contains(fileExtension)) return false;

        return true;
    }

    public static bool IsFullPathValid(string normalizedPath, string targetFolder)
    {
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        string explicitTargetFolder = targetFolder.EndsWith(Path.DirectorySeparatorChar)
            ? targetFolder
            : targetFolder + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(explicitTargetFolder, comparison);
    }

    public static bool IsFileSignatureValid(string extension, ReadOnlySpan<byte> bytes)
    {
        if (!FileSignature.TryGetValue(extension, out var rule))
        {
            return false;
        }

        byte[] signature = rule.Signature;
        int offset = rule.Offset;

        if (bytes.Length < offset + signature.Length)
        {
            return false;
        }

        bool signatureValidation = bytes.Slice(offset, signature.Length).SequenceEqual(signature);

        if (extension != FileFormat.Mp4.ToString("G").ToLower())
        {
            return signatureValidation;
        }
        
        ReadOnlySpan<byte> brandBytes = bytes.Slice(8, 4);
        string brand = Encoding.ASCII.GetString(brandBytes);
        bool brandValidation = AllowedMp4Brands.Contains(brand);

        return signatureValidation && brandValidation;
    }
}