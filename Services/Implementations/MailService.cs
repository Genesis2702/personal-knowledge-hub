using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using PersonalKnowledgeHub.Configuration;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailService(IOptions<MailSettings> options)
    {
        _mailSettings = options.Value;
    }

    public async Task<bool> SendMail(MailData mailData)
    {
        try
        {
            MimeMessage emailMessage = new MimeMessage();
            MailboxAddress emailFrom = new MailboxAddress(_mailSettings.Name, _mailSettings.EmailId);
            emailMessage.From.Add(emailFrom);
            MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToId);
            emailMessage.To.Add(emailTo);
            emailMessage.Subject = mailData.EmailSubject;
            BodyBuilder emailBodyBuilder = new BodyBuilder();
            emailBodyBuilder.HtmlBody = mailData.EmailBody;
            emailMessage.Body = emailBodyBuilder.ToMessageBody();
            
            using SmtpClient mailClient = new SmtpClient();
            await mailClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, _mailSettings.UseSsl);
            await mailClient.AuthenticateAsync(_mailSettings.EmailId, _mailSettings.Password);
            await mailClient.SendAsync(emailMessage);
            await mailClient.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}