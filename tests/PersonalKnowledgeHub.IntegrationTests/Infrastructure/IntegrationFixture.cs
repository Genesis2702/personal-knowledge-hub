using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PersonalKnowledgeHub.Data;
using Respawn;
using Respawn.Graph;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;
    public HttpClient Client { get; private set; } = null!;
    public PersonalKnowledgeHubWebApplicationFactory Factory { get; private set; } = null!;

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
        await _postgreSqlContainer.StartAsync();
        Factory = new PersonalKnowledgeHubWebApplicationFactory(_postgreSqlContainer.GetConnectionString(),
            new IntegrationFactoryOptions
            {
                EnableHangfireServer = false,
                EnableRecurringJobs = false,
                EnableExternalHealthChecks = false
            });
        await ApplyMigrationAsync();
        await InitializeRespawnerAsync();
        Client = Factory.CreateClient();
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
        await _respawner.ResetAsync(_connection);
        var cache = Factory.Services.GetRequiredService<IResettableCache>();
        await cache.ResetAsync();
    }

    private async Task ApplyMigrationAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _connection.DisposeAsync();
        await Factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}