using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class MailFactoryService : IMailFactoryService
{
    public MailData CreateVerificationMail(string emailToId, string emailToName, string verificationToken)
    {
        string verificationLink = $"http://localhost:5165/auth/mail-verification?token={verificationToken}";
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Email Verification",
            EmailBody = $@"
                <h2>Verify Email</h2>
                <a href='{verificationLink}'
                style='padding:12px 18px;background:#2563eb;color:white;text-decoration:none;border-radius:6px;'>
                Verify Account
                </a>"
        };  
    }

    public MailData CreateResetPasswordMail(string emailToId, string emailToName)
    {
        string resetLink = "http://localhost:5165/auth/reset-password";
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Password Reset",
            EmailBody = $@"
                <h2>Verify Email</h2>
                <a href='{resetLink}'
                style='padding:12px 18px;background:#2563eb;color:white;text-decoration:none;border-radius:6px;'>
                Verify Account
                </a>"
        };
    }
}