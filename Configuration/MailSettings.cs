namespace PersonalKnowledgeHub.Configuration;

public class MailSettings
{
    public required string Host { get; set; }
    public int Port { get; set; }
    public required string Name { get; set; }
    public required string EmailId { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public bool UseSsl { get; set; }
}