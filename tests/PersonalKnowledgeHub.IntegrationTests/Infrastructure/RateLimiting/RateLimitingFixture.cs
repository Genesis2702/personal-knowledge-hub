using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.RateLimiting;

public class RateLimitingFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly RedisContainer _redis;
    private bool _disposed;
    
    public HttpClient? Client { get; private set; }
    public PersonalKnowledgeHubWebApplicationFactory? Factory { get; private set; }

    public RateLimitingFixture()
    {
        _postgres = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("personal_knowledge_hub_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _redis = new RedisBuilder("redis:7")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            await _postgres.StartAsync();
            await _redis.StartAsync();
            Factory = new PersonalKnowledgeHubWebApplicationFactory(
                _postgres.GetConnectionString(),
                null,
                new FactoryOptions
                {
                    EnableHangfireServer = false,
                    EnableRecurringJobs = false,
                    EnableHangfireStorage = false,
                    EnableHangfireWrapper = true,
                    EnableRedisWrapper = false,
                    EnableExternalHealthChecks = false
                });
            await ApplyMigrationAsync();
            Client = Factory.CreateClient();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    private async Task ApplyMigrationAsync()
    {
        await using var scope = Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        List<Exception> exceptions = [];

        try
        {
            Client?.Dispose();
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            if (Factory != null) await Factory.DisposeAsync();
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            await _postgres.DisposeAsync();
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more integration fixture resources failed to dispose", exceptions);
        }
    }
}