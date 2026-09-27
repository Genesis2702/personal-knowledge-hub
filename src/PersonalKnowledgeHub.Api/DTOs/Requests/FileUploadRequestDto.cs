using Microsoft.AspNetCore.Mvc;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class FileUploadRequestDto
{
    [FromForm(Name = "file")]
    public required IFormFile FormFile { get; set; }
}