namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Mail;

public sealed class MailpitAddress
{
    public required string Address { get; init; }
    public string? Name { get; init; }
}