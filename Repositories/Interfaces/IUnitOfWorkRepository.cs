using Microsoft.EntityFrameworkCore.Storage;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IUnitOfWorkRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync();
}