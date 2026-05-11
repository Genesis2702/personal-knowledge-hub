using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using PersonalKnowledgeHub.Configuration;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Services.Interfaces;
using Polly.Registry;

namespace PersonalKnowledgeHub.Services.Implementations;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public MailService(IOptions<MailSettings> options, ResiliencePipelineProvider<string> pipeline)
    {
        _mailSettings = options.Value;
        _pipelineProvider = pipeline;
    }

    public async Task<bool> SendMail(MailData mailData)
    {
        try
        {
            var pipeline = _pipelineProvider.GetPipeline("SendMail");
            await pipeline.ExecuteAsync(async cancellationToken =>
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
                await mailClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, _mailSettings.UseSsl, cancellationToken);
                await mailClient.AuthenticateAsync(_mailSettings.EmailId, _mailSettings.Password, cancellationToken);
                await mailClient.SendAsync(emailMessage, cancellationToken);
                await mailClient.DisconnectAsync(true, cancellationToken);
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}