using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.Storage.Implementations.S3;

public static class S3StorageValidator
{
    public static bool IsStoredKeyValid(string storedKey, int userId)
    {
        if (String.IsNullOrEmpty(storedKey)) return false;

        char separator = '/';

        string[] segments = storedKey.Split(separator);

        if (segments.Length != 4) return false;
        
        string userIdSegment = segments[0];
        string yearSegment = segments[1];
        string monthSegment = segments[2];
        
        string[] fileSegments = segments[3].Split('.');

        if (fileSegments.Length != 2) return false;

        string fileName = fileSegments[0];
        string fileExtension = fileSegments[1];

        if (yearSegment.Length != 4) return false;
        if (monthSegment.Length != 2) return false;

        if (Int32.TryParse(userIdSegment, out int id))
        {
            if (id != userId) return false;
            if (id <= 0) return false;
        }
        else return false;

        if (Int32.TryParse(yearSegment, out int year))
        {
            if (year > DateTime.UtcNow.Year) return false;
            if (year < 0) return false;
        }
        else return false;

        if (Int32.TryParse(monthSegment, out int month))
        {
            if (year < DateTime.UtcNow.Year && (month < 1 || month > 12)) return false;
            if (year == DateTime.UtcNow.Year && (month > DateTime.UtcNow.Month || month < 1)) return false;
        }
        else return false;
        
        if (!Guid.TryParse(fileName, out _)) return false;

        fileExtension = fileExtension.ToLowerInvariant();
        if (!FileTypeRegistry.TryGet(fileExtension, out _)) return false;

        return true;
    }
}