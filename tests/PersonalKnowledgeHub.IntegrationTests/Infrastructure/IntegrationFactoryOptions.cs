namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public sealed class IntegrationFactoryOptions
{
    public bool EnableHangfireServer { get; init; }
    public bool EnableRecurringJobs { get; init; }
    public bool EnableExternalHealthChecks { get; init; }
}