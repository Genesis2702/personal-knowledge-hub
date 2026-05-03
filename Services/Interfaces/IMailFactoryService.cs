using PersonalKnowledgeHub.DTOs.Requests;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IMailFactoryService
{
    public MailData CreateVerificationMail(string emailToId, string emailToName, string verificationToken, string userName);
    public MailData CreatePasswordResetMail(string emailToId, string emailToName, string passwordResetToken, string userName);
    public MailData CreatePasswordChangedMail(string emailToId, string emailToName, string userName);
}