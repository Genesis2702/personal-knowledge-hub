using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Options;
using Testcontainers.Mailpit;
using Testcontainers.PostgreSql;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Mail;

public class MailFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly MailpitContainer _mailpit;
    private bool _disposed;
    private const string SmtpUserName = "sender@test.local";
    private const string SmtpPassword = "test-password";
    
    public string MailpitWebAddress => _mailpit.GetWebAddress();
    public HttpClient? MailpitClient { get; private set; }
    
    public HttpClient? Client { get; private set; }
    public PersonalKnowledgeHubWebApplicationFactory? Factory { get; private set; }

    public MailFixture()
    {
        _postgres = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("personal_knowledge_hub_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _mailpit = new MailpitBuilder("axllent/mailpit:v1.30.7")
            .WithSmtpAuthCredentials(
                new NetworkCredential(SmtpUserName, SmtpPassword),
                allowInsecure: true)
            .WithMaxMessages(10)
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            await Task.WhenAll(
                _postgres.StartAsync(),
                _mailpit.StartAsync());
            
            MailpitClient = new HttpClient
            {
                BaseAddress = new Uri(MailpitWebAddress)
            };
            
            Factory = new PersonalKnowledgeHubWebApplicationFactory(
                _postgres.GetConnectionString(),
                null,
                new FactoryOptions
                {
                    EnableHangfireServer = true,
                    EnableRecurringJobs = false,
                    EnableHangfireStorage = true,
                    EnableHangfireWrapper = false,
                    EnableRateLimitMiddleware = false,
                    EnableRedisWrapper = true,
                    EnableExternalHealthChecks = false,
                    Mail = new TestMailOptions
                    {
                        Host = _mailpit.Hostname,
                        Port = _mailpit.SmtpPort,
                        SenderName = "Knowledge Hub Tests",
                        SenderEmail = SmtpUserName,
                        Password = SmtpPassword,
                        UseSsl = false
                    }
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
            MailpitClient?.Dispose();
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            await Task.WhenAll( 
                _postgres.DisposeAsync().AsTask(),
                _mailpit.DisposeAsync().AsTask());
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