using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IMailFactoryService
{
    public MailData CreateVerificationMail(User user, string verificationToken);
    public MailData CreatePasswordResetMail(User user, string passwordResetToken);
    public MailData CreatePasswordChangedMail(User user);
}