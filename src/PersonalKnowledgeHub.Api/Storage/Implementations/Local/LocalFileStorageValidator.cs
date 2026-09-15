using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.Storage.Implementations.Local;

public static class LocalFileStorageValidator
{
    public static bool IsStoredKeyValid(string storedKey, int userId)
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

        fileExtension = fileExtension.ToLowerInvariant();
        FileTypeRegistry.GetRequired(fileExtension);

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
}