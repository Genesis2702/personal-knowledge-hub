using Hangfire;
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
    private readonly string _postgresConnectionString;
    private readonly string _redisConnectionString;
    private readonly FactoryOptions _options;

    public PersonalKnowledgeHubWebApplicationFactory(string postgresConnectionString, string? redisConnectionString, FactoryOptions options)
    {
        _postgresConnectionString = postgresConnectionString;
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            _redisConnectionString = redisConnectionString;
        }
        _options = options;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");
        
        builder.UseSetting("Features:EnableHangfireServer",  _options.EnableHangfireServer.ToString());
        builder.UseSetting("Features:EnableRecurringJobs",  _options.EnableRecurringJobs.ToString());
        builder.UseSetting("Features:EnableHangfireStorage", _options.EnableHangfireStorage.ToString());
        builder.UseSetting("Features:EnableExternalHealthChecks", _options.EnableExternalHealthChecks.ToString());
        builder.UseSetting("Jwt:Key", "72017c9e26c060901a0fd6acfbdeb938");
        builder.UseSetting("Jwt:Issuer", "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresConnectionString);
        builder.UseSetting("RedisCacheSettings:ConnectionString", _redisConnectionString);
        
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgresConnectionString));

            if (!string.IsNullOrEmpty(_redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                    options.Configuration = _redisConnectionString);
            }

            if (_options.EnableRedisWrapper)
            {
                services.RemoveAll<IDistributedCache>();
                services.AddSingleton<MemoryDistributedCache>();
                services.AddSingleton<ResettableCache>();
                services.AddSingleton<IDistributedCache>(provider => provider.GetRequiredService<ResettableCache>());
                services.AddSingleton<IResettableCache>(provider => provider.GetRequiredService<ResettableCache>());
            }

            if (_options.EnableHangfireWrapper)
            {
                services.RemoveAll<IBackgroundJobClient>();
                services.AddSingleton<RecordingBackgroundJobClient>();
                services.AddSingleton<IBackgroundJobClient>(provider =>
                    provider.GetRequiredService<RecordingBackgroundJobClient>());
                services.AddSingleton<IResettableBackgroundJobClient>(provider =>
                    provider.GetRequiredService<RecordingBackgroundJobClient>());
            }
        });
    }
}