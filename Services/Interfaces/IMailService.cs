using PersonalKnowledgeHub.DTOs.Requests;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IMailService
{
    public Task<bool> SendMail(MailData mailData);
}