using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations;

public class UnitOfWorkRepository : IUnitOfWorkRepository
{
    private readonly AppDbContext _dbContext;

    public UnitOfWorkRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async strategyCancellationToken =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(strategyCancellationToken);

            try
            {
                T result = await operation(strategyCancellationToken);
                await transaction.CommitAsync(strategyCancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(strategyCancellationToken);
                throw;
            }
        }, cancellationToken);
    }
}