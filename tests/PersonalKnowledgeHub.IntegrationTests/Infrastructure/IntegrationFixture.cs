using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    public HttpClient Client { get; private set; } = null!;
    public PersonalKnowledgeHubWebApplicationFactory Factory { get; private set; } = null!;

    public IntegrationFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder("postgres:15.1")
            .WithDatabase("personal_knowledge_hub_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        Factory = new PersonalKnowledgeHubWebApplicationFactory(_postgreSqlContainer.GetConnectionString());
        Client = Factory.CreateClient();
        await ApplyMigrationAsync();
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
        await Factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}