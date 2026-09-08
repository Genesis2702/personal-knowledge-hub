using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Mapper;

public static class StoredFileMapper
{
    public static StoredFileResponseDto ToStoredFileResponseDto(StoredFile storedFile)
    {
        return new StoredFileResponseDto
        {
            FileName = storedFile.FileName,
            SizeInBytes = storedFile.SizeInBytes,
            ContentType = storedFile.ContentType,
            ResourceId = storedFile.ResourceId,
            FileFormat = storedFile.FileFormat
        };
    }
}