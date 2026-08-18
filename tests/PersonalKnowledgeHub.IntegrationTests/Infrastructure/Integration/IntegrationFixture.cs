using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Options;
using Respawn;
using Respawn.Graph;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private NpgsqlConnection? _connection;
    private Respawner? _respawner;
    private bool _disposed;
    public HttpClient? Client { get; private set; }
    public PersonalKnowledgeHubWebApplicationFactory? Factory { get; private set; }

    public IntegrationFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("personal_knowledge_hub_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            await _postgreSqlContainer.StartAsync();
            Factory = new PersonalKnowledgeHubWebApplicationFactory(
                _postgreSqlContainer.GetConnectionString(),
                null,
                new FactoryOptions
                {
                    EnableHangfireServer = false,
                    EnableRecurringJobs = false,
                    EnableHangfireStorage = false,
                    EnableHangfireWrapper = true,
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
        _connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore =
                [
                    new Table("__EFMigrationsHistory")
                ]
            });
    }

    public async Task ResetStateAsync()
    {
        await _respawner!.ResetAsync(_connection!);
        await Factory!.Services.GetRequiredService<IResettableCache>().ResetAsync();
        Factory.Services.GetRequiredService<IResettableBackgroundJobClient>().Reset();
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
            await _postgreSqlContainer.DisposeAsync();
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