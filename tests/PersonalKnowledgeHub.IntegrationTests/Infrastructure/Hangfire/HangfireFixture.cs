using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PersonalKnowledgeHub.Data;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Hangfire;

public class HangfireFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private NpgsqlConnection? _connection;
    private Respawner? _respawner;
    private bool _disposed;
    
    public HttpClient? Client { get; private set; }
    public PersonalKnowledgeHubWebApplicationFactory? Factory { get; private set; }

    public HangfireFixture()
    {
        _postgres = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("personal_knowledge_hub_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            await _postgres.StartAsync();
            Factory = new PersonalKnowledgeHubWebApplicationFactory(
                _postgres.GetConnectionString(),
                null,
                new FactoryOptions
                {
                    EnableHangfireServer = true,
                    EnableRecurringJobs = false,
                    EnableHangfireStorage = true,
                    EnableHangfireWrapper = false,
                    EnableRedisWrapper = true,
                    EnableExternalHealthChecks = false
                });
            await ApplyMigrationAsync();
            await InitializeRespawnerAsync();
            Client = Factory.CreateClient();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }
    
    private async Task InitializeRespawnerAsync()
    {
        _connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public", "hangfire"],
                TablesToIgnore =
                [
                    new Table("__EFMigrationsHistory")
                ]
            });
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
            if (_connection != null) await _connection.DisposeAsync();
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