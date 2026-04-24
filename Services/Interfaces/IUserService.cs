using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IUserService
{
    public Task<PageResult<User>> GetUsers(int pageIndex, int pageSize, UserStatus? status);
    public Task<User> GetUserById(int id);
    public Task BanUser(int userId);
    public Task UnbanUser(int userId);
}