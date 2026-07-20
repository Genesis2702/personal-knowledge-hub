using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersonalKnowledgeHub.Data;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public class PersonalKnowledgeHubWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public PersonalKnowledgeHubWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");
        
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });
    }
}