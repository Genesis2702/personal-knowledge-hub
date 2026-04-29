using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class MailFactoryService : IMailFactoryService
{
    public MailData CreateVerificationMail(string emailToId, string emailToName)
    {
        string verificationLink = "frontend link";
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Email Verification",
            EmailBody = $"Email Verification: {verificationLink}"
        };  
    }

    public MailData CreateResetPasswordMail(string emailToId, string emailToName)
    {
        string resetLink = "frontend link";
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Password Reset",
            EmailBody = $"Password Reset: {resetLink}"
        };
    }
}