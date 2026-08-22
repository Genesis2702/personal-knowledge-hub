namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Mail;

public sealed class MailpitMessage
{
    public required MailpitAddress From { get; init; }
    public required List<MailpitAddress> To { get; init; }
    public required string Subject { get; init; }
    public required string Html { get; init; }
}