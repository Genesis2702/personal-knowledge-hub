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
                  <meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <title>Email Verification</title>
                </head>

                <body style=""margin:0;padding:0;background-color:#10076B;font-family:Arial,Helvetica,sans-serif;"">

                  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:32px 12px;"">
                    <tr>
                      <td align=""center"">

                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                          style=""
                            max-width:640px;
                            background-color:#0B0638;
                            border:1px solid #241B86;
                            border-radius:16px;
                            overflow:hidden;
                          "">

                          <tr>
                            <td style=""padding:38px 32px 42px 32px;text-align:center;"">

                              <div style=""
                                color:#8ACBD0;
                                font-size:20px;
                                font-weight:700;
                                margin-bottom:26px;
                                letter-spacing:0.2px;
                              "">
                                Knowledge Hub
                              </div>

                              <h1 style=""
                                margin:0;
                                color:#EFE3CA;
                                font-size:40px;
                                line-height:1.25;
                                font-weight:500;
                              "">
                                Verify your email
                              </h1>

                              <div style=""margin-top:20px;"">
                                <span style=""
                                  display:inline-block;
                                  padding:10px 16px;
                                  border:1px solid #241B86;
                                  border-radius:999px;
                                  color:#D7D9E8;
                                  font-size:15px;
                                  line-height:1.4;
                                  background-color:#12085E;
                                "">
                                  {emailToId}
                                </span>
                              </div>

                              <div style=""
                                height:1px;
                                background-color:#241B86;
                                margin:30px 0 30px 0;
                              ""></div>

                              <div style=""
                                color:#D7D9E8;
                                font-size:16px;
                                line-height:1.75;
                                text-align:left;
                              "">
                                Hello <strong style=""color:#EFE3CA;"">{userName}</strong>,<br><br>

                                Welcome to <strong style=""color:#EFE3CA;"">Knowledge Hub</strong>.<br><br>

                                Please verify your email address to activate your account and start using the platform.
                              </div>

                              <div style=""margin-top:34px;"">
                                <a href=""{verificationLink}""
                                  style=""
                                    display:inline-block;
                                    background-color:#56B6C6;
                                    color:#0B0638;
                                    text-decoration:none;
                                    padding:15px 34px;
                                    border-radius:999px;
                                    font-size:16px;
                                    font-weight:700;
                                  "">
                                  Verify email
                                </a>
                              </div>

                              <div style=""
                                margin-top:28px;
                                color:#D7D9E8;
                                font-size:15px;
                                line-height:1.7;
                                text-align:center;
                              "">
                                This verification link will expire in <strong style=""color:#EFE3CA;"">24 hours</strong>.
                              </div>

                              <div style=""
                                margin-top:26px;
                                color:#9CA3AF;
                                font-size:14px;
                                line-height:1.7;
                                text-align:center;
                              "">
                                If you did not create this account, you can safely ignore this email.
                              </div>

                            </td>
                          </tr>

                        </table>

                      </td>
                    </tr>
                  </table>

                </body>
                </html>
            "
        };  
    }

    public MailData CreatePasswordResetMail(string emailToId, string emailToName, string passwordResetToken, string userName)
    {
        string resetPasswordLink = $"http://localhost:5165/auth/reset-password?token={passwordResetToken}";
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Password Reset",
            EmailBody = $@"
              <!DOCTYPE html>
              <html>
              <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Password Reset Request</title>
              </head>

              <body style=""margin:0;padding:0;background-color:#0B1020;font-family:Arial,Helvetica,sans-serif;"">

                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:32px 12px;"">
                  <tr>
                    <td align=""center"">

                      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                        style=""
                          max-width:640px;
                          background-color:#111827;
                          border:1px solid #2A3446;
                          border-radius:16px;
                          overflow:hidden;
                        "">

                        <tr>
                          <td style=""padding:36px 30px 40px 30px;text-align:center;"">

                            <div style=""
                              color:#56B6C6;
                              font-size:20px;
                              font-weight:700;
                              margin-bottom:26px;
                              letter-spacing:0.2px;
                            "">
                              Knowledge Hub
                            </div>

                            <h1 style=""
                              margin:0;
                              color:#F3F4F6;
                              font-size:40px;
                              line-height:1.25;
                              font-weight:500;
                            "">
                              Reset your password
                            </h1>

                            <div style=""margin-top:20px;"">
                              <span style=""
                                display:inline-block;
                                padding:10px 16px;
                                border:1px solid #2A3446;
                                border-radius:999px;
                                color:#D1D5DB;
                                font-size:15px;
                                line-height:1.4;
                                background-color:#0F172A;
                              "">
                                {emailToId}
                              </span>
                            </div>

                            <div style=""
                              height:1px;
                              background-color:#2A3446;
                              margin:28px 0 30px 0;
                            ""></div>

                            <div style=""
                              color:#D1D5DB;
                              font-size:16px;
                              line-height:1.75;
                              text-align:left;
                            "">
                              Hello <strong style=""color:#FFFFFF;"">{userName}</strong>,<br><br>

                              We received a request to reset the password for your <strong style=""color:#FFFFFF;"">Knowledge Hub</strong> account.<br><br>

                              To continue, use the button below. For your security, this link should only be used by you.
                            </div>

                            <div style=""margin-top:34px;"">
                              <a href=""{resetPasswordLink}""
                                style=""
                                  display:inline-block;
                                  background-color:#56B6C6;
                                  color:#0B1020;
                                  text-decoration:none;
                                  padding:15px 32px;
                                  border-radius:999px;
                                  font-size:16px;
                                  font-weight:700;
                                "">
                                Reset password
                              </a>
                            </div>
                            
                            <div style=""margin-top:18px;color:#9CA3AF;font-size:14px;line-height:1.6;text-align:center;"">
                This password reset link will expire shortly for your security.
              </div>

                            <div style=""
                              margin-top:28px;
                              color:#9CA3AF;
                              font-size:15px;
                              line-height:1.7;
                              text-align:center;
                            "">
                              If you did not request this password reset, you can safely ignore this email.
                            </div>

                            <div style=""
                              margin-top:26px;
                              padding-top:24px;
                              border-top:1px solid #2A3446;
                              color:#9CA3AF;
                              font-size:14px;
                              line-height:1.7;
                              text-align:center;
                            "">
                              This is an automated security notification from Knowledge Hub.
                            </div>

                          </td>
                        </tr>

                      </table>

                    </td>
                  </tr>
                </table>

              </body>
              </html>
            "
        };
    }

    public MailData CreatePasswordChangedMail(string emailToId, string emailToName, string userName)
    {
        return new MailData
        {
            EmailToId = emailToId,
            EmailToName = emailToName,
            EmailSubject = "Password Changed",
            EmailBody = $@"
              <!DOCTYPE html>
              <html>
              <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Password Changed</title>
              </head>

              <body style=""margin:0;padding:0;background-color:#0B1020;font-family:Arial,Helvetica,sans-serif;"">

                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:32px 12px;"">
                  <tr>
                    <td align=""center"">

                      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                        style=""
                          max-width:640px;
                          background-color:#111827;
                          border:1px solid #2A3446;
                          border-radius:16px;
                          overflow:hidden;
                        "">

                        <tr>
                          <td style=""padding:36px 30px 40px 30px;text-align:center;"">

                            <div style=""
                              color:#56B6C6;
                              font-size:20px;
                              font-weight:700;
                              margin-bottom:26px;
                              letter-spacing:0.2px;
                            "">
                              Knowledge Hub
                            </div>

                            <h1 style=""
                              margin:0;
                              color:#F3F4F6;
                              font-size:40px;
                              line-height:1.25;
                              font-weight:500;
                            "">
                              Password updated
                            </h1>

                            <div style=""margin-top:20px;"">
                              <span style=""
                                display:inline-block;
                                padding:10px 16px;
                                border:1px solid #2A3446;
                                border-radius:999px;
                                color:#D1D5DB;
                                font-size:15px;
                                line-height:1.4;
                                background-color:#0F172A;
                              "">
                                {emailToId}
                              </span>
                            </div>

                            <div style=""
                              height:1px;
                              background-color:#2A3446;
                              margin:28px 0 30px 0;
                            ""></div>

                            <div style=""
                              color:#D1D5DB;
                              font-size:16px;
                              line-height:1.75;
                              text-align:left;
                            "">
                              Hello <strong style=""color:#FFFFFF;"">{userName}</strong>,<br><br>

                              The password for your <strong style=""color:#FFFFFF;"">Knowledge Hub</strong> account was changed successfully.<br><br>

                              If you made this change, no further action is needed.
                            </div>

                            <div style=""
                              margin-top:30px;
                              background-color:#0F172A;
                              border:1px solid #2A3446;
                              border-radius:12px;
                              padding:18px;
                              color:#D1D5DB;
                              font-size:15px;
                              line-height:1.7;
                              text-align:left;
                            "">
                              <strong style=""color:#F3F4F6;"">Didn’t change your password?</strong><br>
                              Your account may be at risk. Please contact support immediately to request account recovery assistance.
                            </div>

                            <div style=""
                              margin-top:26px;
                              padding-top:24px;
                              border-top:1px solid #2A3446;
                              color:#9CA3AF;
                              font-size:14px;
                              line-height:1.7;
                              text-align:center;
                            "">
                              This is an automated security notification from Knowledge Hub.
                            </div>

                          </td>
                        </tr>

                      </table>

                    </td>
                  </tr>
                </table>

              </body>
              </html>
            "
        };
    }
}