using Microsoft.EntityFrameworkCore.Storage;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IUnitOfWorkRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken);
}