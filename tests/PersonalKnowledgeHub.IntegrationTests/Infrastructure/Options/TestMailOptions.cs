namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Options;

public sealed class TestMailOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string SenderName { get; init; }
    public required string SenderEmail { get; init; }
    public required string Password { get; init; }
    public bool UseSsl { get; init; }
}