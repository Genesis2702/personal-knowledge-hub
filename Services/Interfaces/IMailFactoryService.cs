using PersonalKnowledgeHub.DTOs.Requests;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IMailFactoryService
{
    public MailData CreateVerificationMail(string emailToId, string emailToName, string verificationToken, string userName);
    public MailData CreateResetPasswordMail(string emailToId, string emailToName, string userName);
}