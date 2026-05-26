using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class MailFactoryService : IMailFactoryService
{
    public MailData CreateVerificationMail(User user, string verificationToken)
    {
        string verificationLink = $"http://localhost:5165/auth/mail-verification?token={verificationToken}";
        return new MailData
        {
            EmailToId = user.Email,
            EmailToName = user.UserName ?? user.Email,
            EmailSubject = "Email Verification",
            EmailBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset=""UTF-8"" />
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                  <title>Verify Your Email</title>
                </head>

                <body style=""margin:0;padding:0;background-color:#f4f8fb;font-family:Arial,Helvetica,sans-serif;color:#384959;"">

                  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:40px 16px;"">
                    <tr>
                      <td align=""center"">

                        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                          style=""max-width:600px;background-color:#ffffff;border:1px solid #BDDDFC;border-radius:16px;padding:42px;box-shadow:0 10px 28px rgba(56,73,89,0.08);"">

                          <tr>
                            <td align=""center"" style=""padding-bottom:28px;"">
                              <h2 style=""margin:0;color:#88BDF2;font-size:24px;font-weight:700;letter-spacing:0.2px;"">
                                Knowledge Hub
                              </h2>
                            </td>
                          </tr>

                          <tr>
                            <td align=""center"" style=""padding-bottom:10px;"">
                              <h1 style=""margin:0;color:#88BDF2;font-size:28px;font-weight:700;"">
                                Verify your email
                              </h1>
                            </td>
                          </tr>

                          <tr>
                            <td align=""center"" style=""padding-bottom:28px;"">
                              <p style=""margin:0;color:#6A89A7;font-size:14px;line-height:1.6;"">
                                {user.Email}
                              </p>
                            </td>
                          </tr>

                          <tr>
                            <td>
                              <p style=""margin:0 0 16px;color:#384959;font-size:16px;line-height:1.7;"">
                                Hello {user.UserName ?? user.Email},
                              </p>

                              <p style=""margin:0 0 24px;color:#384959;font-size:16px;line-height:1.7;"">
                                Welcome to Knowledge Hub. Please verify your email address to activate your account and start managing, sharing, and discovering learning resources.
                              </p>

                              <p style=""margin:0 0 30px;color:#6A89A7;font-size:15px;line-height:1.7;"">
                                This helps us keep your account secure and make sure important account updates reach the right email address.
                              </p>
                            </td>
                          </tr>

                          <tr>
                            <td align=""center"" style=""padding-bottom:30px;"">
                              <a href=""{verificationLink}""
                                style=""display:inline-block;background-color:#384959;color:#BDDDFC;text-decoration:none;font-size:15px;font-weight:700;padding:14px 28px;border-radius:10px;"">
                                Verify Email
                              </a>
                            </td>
                          </tr>

                          <tr>
                            <td>
                              <p style=""margin:0;color:#6A89A7;font-size:13px;line-height:1.6;"">
                                If you did not create a Knowledge Hub account, you can safely ignore this email.
                              </p>
                            </td>
                          </tr>

                        </table>

                        <p style=""margin:24px 0 0;color:#6A89A7;font-size:12px;line-height:1.6;"">
                          © Knowledge Hub. Built for better learning and resource sharing.
                        </p>

                      </td>
                    </tr>
                  </table>

                </body>
                </html>
            "
        };  
    }

    public MailData CreatePasswordResetMail(User user, string passwordResetToken)
    {
        string resetPasswordLink = $"http://localhost:5165/auth/reset-password?token={passwordResetToken}";
        return new MailData
        {
            EmailToId = user.Email,
            EmailToName = user.UserName ?? user.Email,
            EmailSubject = "Password Reset",
            EmailBody = $@"
              <!DOCTYPE html>
              <html>
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Reset Your Password</title>
              </head>

              <body style=""margin:0;padding:0;background-color:#f4f8fb;font-family:Arial,Helvetica,sans-serif;color:#384959;"">

                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:40px 16px;"">
                  <tr>
                    <td align=""center"">

                      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                        style=""max-width:600px;background-color:#ffffff;border:1px solid #BDDDFC;border-radius:16px;padding:42px;box-shadow:0 10px 28px rgba(56,73,89,0.08);"">

                        <tr>
                          <td align=""center"" style=""padding-bottom:28px;"">
                            <h2 style=""margin:0;color:#88BDF2;font-size:24px;font-weight:700;letter-spacing:0.2px;"">
                              Knowledge Hub
                            </h2>
                          </td>
                        </tr>

                        <tr>
                          <td align=""center"" style=""padding-bottom:10px;"">
                            <h1 style=""margin:0;color:#88BDF2;font-size:28px;font-weight:700;"">
                              Reset your password
                            </h1>
                          </td>
                        </tr>

                        <tr>
                          <td align=""center"" style=""padding-bottom:28px;"">
                            <p style=""margin:0;color:#6A89A7;font-size:14px;line-height:1.6;"">
                              {user.Email}
                            </p>
                          </td>
                        </tr>

                        <tr>
                          <td>
                            <p style=""margin:0 0 16px;color:#384959;font-size:16px;line-height:1.7;"">
                              Hello {user.UserName ?? user.Email},
                            </p>

                            <p style=""margin:0 0 24px;color:#384959;font-size:16px;line-height:1.7;"">
                              We received a request to reset the password for your Knowledge Hub account. You can create a new password by clicking the button below.
                            </p>

                            <p style=""margin:0 0 30px;color:#6A89A7;font-size:15px;line-height:1.7;"">
                              For your security, this password reset link should only be used by you. If you did not request this, no changes will be made to your account.
                            </p>
                          </td>
                        </tr>

                        <tr>
                          <td align=""center"" style=""padding-bottom:30px;"">
                            <a href=""{resetPasswordLink}""
                              style=""display:inline-block;background-color:#384959;color:#BDDDFC;text-decoration:none;font-size:15px;font-weight:700;padding:14px 28px;border-radius:10px;"">
                              Reset Password
                            </a>
                          </td>
                        </tr>

                        <tr>
                          <td>
                            <p style=""margin:0;color:#6A89A7;font-size:13px;line-height:1.6;"">
                              If you did not request a password reset, you can safely ignore this email.
                            </p>
                          </td>
                        </tr>

                      </table>

                      <p style=""margin:24px 0 0;color:#6A89A7;font-size:12px;line-height:1.6;"">
                        © Knowledge Hub. Built for better learning and resource sharing.
                      </p>

                    </td>
                  </tr>
                </table>

              </body>
              </html>
            "
        };
    }

    public MailData CreatePasswordChangedMail(User user)
    {
        return new MailData
        {
            EmailToId = user.Email,
            EmailToName = user.UserName ?? user.Email,
            EmailSubject = "Password Changed",
            EmailBody = $@"
              <!DOCTYPE html>
              <html>
              <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Password Updated</title>
              </head>

              <body style=""margin:0;padding:0;background-color:#f4f8fb;font-family:Arial,Helvetica,sans-serif;color:#384959;"">

                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:40px 16px;"">
                  <tr>
                    <td align=""center"">

                      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                        style=""max-width:600px;background-color:#ffffff;border:1px solid #BDDDFC;border-radius:16px;padding:42px;box-shadow:0 10px 28px rgba(56,73,89,0.08);"">

                        <tr>
                          <td align=""center"" style=""padding-bottom:28px;"">
                            <h2 style=""margin:0;color:#88BDF2;font-size:24px;font-weight:700;letter-spacing:0.2px;"">
                              Knowledge Hub
                            </h2>
                          </td>
                        </tr>

                        <tr>
                          <td align=""center"" style=""padding-bottom:10px;"">
                            <h1 style=""margin:0;color:#88BDF2;font-size:28px;font-weight:700;"">
                              Password updated
                            </h1>
                          </td>
                        </tr>

                        <tr>
                          <td align=""center"" style=""padding-bottom:28px;"">
                            <p style=""margin:0;color:#6A89A7;font-size:14px;line-height:1.6;"">
                              {user.Email}
                            </p>
                          </td>
                        </tr>

                        <tr>
                          <td>
                            <p style=""margin:0 0 16px;color:#384959;font-size:16px;line-height:1.7;"">
                              Hello {user.UserName ?? user.Email},
                            </p>

                            <p style=""margin:0 0 24px;color:#384959;font-size:16px;line-height:1.7;"">
                              Your Knowledge Hub account password has been updated successfully.
                            </p>

                            <p style=""margin:0 0 24px;color:#6A89A7;font-size:15px;line-height:1.7;"">
                              This email is sent to confirm that the change was completed. You can continue using Knowledge Hub to organize, manage, and share your learning resources.
                            </p>

                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0""
                              style=""background-color:#f4f8fb;border:1px solid #BDDDFC;border-radius:12px;padding:18px;margin:26px 0;"">
                              <tr>
                                <td>
                                  <p style=""margin:0;color:#384959;font-size:14px;line-height:1.7;"">
                                    If you did not update your password, please secure your account immediately or contact support.
                                  </p>
                                </td>
                              </tr>
                            </table>

                            <p style=""margin:0;color:#6A89A7;font-size:13px;line-height:1.6;"">
                              For your safety, we recommend using a strong password that you do not reuse on other websites.
                            </p>
                          </td>
                        </tr>

                      </table>

                      <p style=""margin:24px 0 0;color:#6A89A7;font-size:12px;line-height:1.6;"">
                        © Knowledge Hub. Built for better learning and resource sharing.
                      </p>

                    </td>
                  </tr>
                </table>

              </body>
              </html>
            "
        };
    }
}