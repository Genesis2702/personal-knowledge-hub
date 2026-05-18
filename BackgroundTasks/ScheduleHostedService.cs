using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.BackgroundTasks;

public class ScheduleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduleHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = _scopeFactory.CreateScope();
            var refreshTokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var verificationTokenService = scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
            await refreshTokenService.CleanUpRefreshTokens(stoppingToken);
            await verificationTokenService.CleanUpVerificationTokens(stoppingToken);
            await Task.Delay(TimeSpan.FromDays(30), stoppingToken);
        }
    }
}