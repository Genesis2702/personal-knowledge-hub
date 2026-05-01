using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class MailFactoryService : IMailFactoryService
{
    public MailData CreateVerificationMail(string emailToId, string emailToName, string verificationToken, string userName)
    {
        string verificationLink = $"http://localhost:5165/auth/mail-verification?token={verificationToken}";
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Email Verification",
            EmailBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Email Verification</title>
                </head>

                <body style='margin:0;padding:0;background-color:#0f0f13;font-family:Arial,Helvetica,sans-serif;'>

                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='padding:40px 0;'>
                <tr>
                <td align='center'>

                <table role='presentation' width='600' cellspacing='0' cellpadding='0'
                style='background-color:#18181b;
                border:1px solid #2a2a31;
                border-radius:14px;
                padding:42px;
                box-shadow:0 12px 28px rgba(0,0,0,0.45);'>

                <tr>
                <td align='center'>

                <h1 style='margin:0;
                color:#c084fc;
                font-size:30px;
                font-weight:700;'>

                Email Verification

                </h1>

                </td>
                </tr>

                <tr>
                <td style='padding-top:28px;
                color:#e4e4e7;
                font-size:16px;
                line-height:1.8;
                text-align:left;'>

                Hello <strong style='color:#ffffff;'>{userName}</strong>,<br><br>

                Welcome to <strong style='color:#ffffff;'>Knowledge Hub</strong>.<br><br>

                Please verify your email address to activate your account and unlock full access to the platform.<br><br>

                If your email is not verified within <strong style='color:#ffffff;'>24 hours</strong>, your account will become inactive until verification is completed.

                </td>
                </tr>

                <tr>
                <td align='center' style='padding-top:32px;'>

                <a href='{verificationLink}'
                style='background-color:#a855f7;
                color:#ffffff;
                text-decoration:none;
                padding:14px 34px;
                border-radius:10px;
                font-size:16px;
                font-weight:700;
                display:inline-block;
                box-shadow:0 6px 18px rgba(168,85,247,0.35);'>

                Verify

                </a>

                </td>
                </tr>

                <tr>
                <td align='center'
                style='padding-top:26px;
                color:#71717a;
                font-size:13px;
                line-height:1.6;'>

                If you did not create this account, you may safely ignore this email.

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>"
        };  
    }

    public MailData CreateResetPasswordMail(string emailToId, string emailToName, string userName)
    {
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Password Reset",
            EmailBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Changed</title>
                </head>

                <body style='margin:0;padding:0;background-color:#0f0f13;font-family:Arial,Helvetica,sans-serif;'>

                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='padding:40px 0;'>
                <tr>
                <td align='center'>

                <table role='presentation' width='600' cellspacing='0' cellpadding='0'
                style='background-color:#18181b;
                border:1px solid #2a2a31;
                border-radius:14px;
                padding:42px;
                box-shadow:0 12px 28px rgba(0,0,0,0.45);'>

                <tr>
                <td align='center'>

                <h1 style='margin:0;
                color:#c084fc;
                font-size:30px;
                font-weight:700;'>

                Password Updated

                </h1>

                </td>
                </tr>

                <tr>
                <td style='padding-top:28px;
                color:#e4e4e7;
                font-size:16px;
                line-height:1.8;
                text-align:left;'>

                Hello <strong style='color:#ffffff;'>{userName}</strong>,<br><br>

                The password for your <strong style='color:#ffffff;'>Knowledge Hub</strong> account has been changed successfully.<br><br>

                If this action was not performed by you, your account may be at risk.<br><br>

                <strong style='color:#ffffff;'>Recommended Action:</strong><br>
                Please contact our support service immediately to request account recovery assistance.

                </td>
                </tr>

                <tr>
                <td align='center'
                style='padding-top:26px;
                color:#71717a;
                font-size:13px;
                line-height:1.6;'>

                This is an automated security notification.

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>"
        };
    }
}