using Hangfire;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.BackgroundTasks;

public static class RecurringTaskService
{
    public static void RegisterRecurringJobs(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        
        recurringJobManager.AddOrUpdate<ITokenService>(
            "token-cleanup", 
            tokenService => tokenService.CleanUpRefreshTokens(CancellationToken.None),
            Cron.Monthly);
        recurringJobManager.AddOrUpdate<IVerificationTokenService>(
            "verification-token-cleanup",
            verificationTokenService => verificationTokenService.CleanUpVerificationTokens(CancellationToken.None),
            Cron.Monthly);
        recurringJobManager.AddOrUpdate<IResourceService>(
            "resource-cleanup",
            resourceService => resourceService.CleanUpResources(CancellationToken.None),
            Cron.Monthly);
        recurringJobManager.AddOrUpdate<ITagService>(
            "tag-cleanup",
            tagService => tagService.CleanUpTags(CancellationToken.None),
            Cron.Monthly);
    }
}