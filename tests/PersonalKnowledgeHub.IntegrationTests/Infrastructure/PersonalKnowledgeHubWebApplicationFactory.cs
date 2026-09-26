using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Options;
using PersonalKnowledgeHub.Storage.Implementations.Supabase;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public class PersonalKnowledgeHubWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string? _postgresConnectionString;
    private readonly string? _redisConnectionString;
    private readonly FactoryOptions _options;

    public PersonalKnowledgeHubWebApplicationFactory(string? postgresConnectionString, string? redisConnectionString, FactoryOptions options)
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
        builder.UseSetting("Features:EnableRateLimitMiddleware", _options.EnableRateLimitMiddleware.ToString());
        builder.UseSetting("Features:EnableExternalHealthChecks", _options.EnableExternalHealthChecks.ToString());
        builder.UseSetting("Jwt:Key", "72017c9e26c060901a0fd6acfbdeb938");
        builder.UseSetting("Jwt:Issuer", "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresConnectionString);
        builder.UseSetting("FileUploadOptions:MaxFileSizeInBytes", (10 * 1024 * 1024).ToString());

        if (_redisConnectionString is not null)
        {
            builder.UseSetting("RedisCacheSettings:ConnectionString", _redisConnectionString);
        }

        if (_options.Mail is not null)
        {
            builder.UseSetting("MailSettings:Host", _options.Mail.Host);
            builder.UseSetting(
                "MailSettings:Port",
                _options.Mail.Port.ToString());

            builder.UseSetting("MailSettings:Name", _options.Mail.SenderName);
            builder.UseSetting("MailSettings:EmailId", _options.Mail.SenderEmail);
            builder.UseSetting("MailSettings:UserName", _options.Mail.SenderEmail);
            builder.UseSetting("MailSettings:Password", _options.Mail.Password);
            builder.UseSetting(
                "MailSettings:UseSsl",
                _options.Mail.UseSsl.ToString());
        }

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddUserSecrets<Program>(optional: _options.Provider != StorageProvider.Supabase);
        });
        
        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgresConnectionString));

            switch (_options.Provider)
            {
                case StorageProvider.Resettable:
                    services.RemoveAll<IFileStorage>();
                    
                    services.AddSingleton<ResettableFileStorage>();
                    
                    services.AddSingleton<IFileStorage>(provider =>
                        provider.GetRequiredService<ResettableFileStorage>());
                    services.AddSingleton<IResettableFileStorage>(provider =>
                        provider.GetRequiredService<ResettableFileStorage>());
                    break;
                
                case StorageProvider.Supabase:
                    services.RemoveAll<IFileStorage>();
                    services.RemoveAll<Supabase.Client>();
                    services.RemoveAll<IOptions<SupabaseStorageOptions>>();
                    
                    string url = context.Configuration["Supabase:Url"] ??
                                 throw new InvalidOperationException("Supabase url is not configured");
                    string key = context.Configuration["Supabase:Key"] ??
                                 throw new InvalidOperationException("Supabase key is not configured");
                    services.AddSingleton(new Supabase.Client(url, key));

                    services.AddSingleton<IOptions<SupabaseStorageOptions>>(Microsoft.Extensions.Options.Options.Create(
                        new SupabaseStorageOptions
                        {
                            BucketName = "personal-knowledge-hub-test"
                        }));

                    services.AddScoped<IFileStorage, SupabaseStorage>();
                    break;
                
                default:
                    throw new InvalidOperationException("Not supported storage provider");
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