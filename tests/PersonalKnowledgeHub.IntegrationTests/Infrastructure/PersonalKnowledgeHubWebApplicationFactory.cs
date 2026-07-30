using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersonalKnowledgeHub.Data;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public class PersonalKnowledgeHubWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly IntegrationFactoryOptions _options;

    public PersonalKnowledgeHubWebApplicationFactory(string connectionString, IntegrationFactoryOptions options)
    {
        _connectionString = connectionString;
        _options = options;
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

            services.RemoveAll<IDistributedCache>();
            services.AddSingleton<MemoryDistributedCache>();
            services.AddSingleton<ResettableCache>();
            services.AddSingleton<IDistributedCache>(provider => provider.GetRequiredService<ResettableCache>());
            services.AddSingleton<IResettableCache>(provider => provider.GetRequiredService<ResettableCache>());
        });

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "72017c9e26c060901a0fd6acfbdeb938",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["ConnectionStrings:DefaultConnection"] =  _connectionString,
                    ["Features:EnableHangfireServer"] = _options.EnableHangfireServer.ToString(),
                    ["Features:EnableRecurringJobs"] = _options.EnableRecurringJobs.ToString(),
                    ["Features:EnableExternalHealthChecks"] = _options.EnableExternalHealthChecks.ToString()
                });
        });
    }
}