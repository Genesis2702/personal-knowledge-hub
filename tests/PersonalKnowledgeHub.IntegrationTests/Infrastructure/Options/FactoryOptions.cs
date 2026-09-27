namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Options;

public sealed class FactoryOptions
{
    public bool EnableHangfireServer { get; init; }
    public bool EnableRecurringJobs { get; init; }
    public bool EnableHangfireStorage { get; init; }
    public bool EnableHangfireWrapper { get; init; }
    public bool EnableRateLimitMiddleware { get; init; }
    public bool EnableRedisWrapper { get; init; }
    public bool EnableExternalHealthChecks { get; init; }
    public TestMailOptions? Mail { get; init; }
    public StorageProvider Provider { get; init; } = StorageProvider.Resettable;
}

public enum StorageProvider
{
    Resettable, 
    Supabase
}