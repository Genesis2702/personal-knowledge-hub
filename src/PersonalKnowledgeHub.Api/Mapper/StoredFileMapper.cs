using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Models;

namespace PersonalKnowledgeHub.Mapper;

public static class StoredFileMapper
{
    public static StoredFile ToStoredFile(string fileName, FileResult fileResult, int resourceId)
    {
        return new StoredFile
        {
            FileName = fileName,
            StoredKey = fileResult.StoredKey,
            SizeInBytes = fileResult.SizeInBytes,
            ContentType = fileResult.ContentType,
            ResourceId = resourceId,
            FileFormat = fileResult.FileFormat
        };
    }
    
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