using PersonalKnowledgeHub.DTOs.Requests;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IMailFactoryService
{
    public MailData CreateVerificationMail(string emailToId, string emailToNamem, string verificationToken);
    public MailData CreateResetPasswordMail(string emailToId, string emailToName);
}