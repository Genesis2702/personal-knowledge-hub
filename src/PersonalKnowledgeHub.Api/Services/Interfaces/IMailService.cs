using PersonalKnowledgeHub.DTOs.Requests;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IMailService
{
    public Task SendMail(MailData mailData);
}