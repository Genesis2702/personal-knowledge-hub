using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IUserService
{
    public Task<PageResult<User>> GetUsers(int pageIndex, int pageSize, UserStatus? status, CancellationToken cancellationToken);
    public Task<User> GetUserById(int id, CancellationToken cancellationToken);
    public Task UpdateUserName(int id, UserUpdateRequestDto userUpdateRequest, CancellationToken cancellationToken);
    public Task BanUser(int userId, CancellationToken cancellationToken);
    public Task UnbanUser(int userId, CancellationToken cancellationToken);
    public Task<User> AddRoleToUser(int userId, int roleId, CancellationToken cancellationToken);
    public Task RemoveRoleFromUser(int userId, int roleId, CancellationToken cancellationToken);
}