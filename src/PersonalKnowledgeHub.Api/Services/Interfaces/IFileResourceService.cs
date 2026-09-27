using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Models;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IFileResourceService
{
    public Task<Resource> CreateFileResource(IFormFile file, int userId, CancellationToken cancellationToken);
    public Task<FileDownloadResult> OpenFileResource(int resourceId, int userId, CancellationToken cancellationToken);
    public Task DeleteFileResourcePermanently(CancellationToken cancellationToken);
}