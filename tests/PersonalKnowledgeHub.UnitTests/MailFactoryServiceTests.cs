using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class MailFactoryServiceTests
{
    private readonly IMailFactoryService _mailFactoryService;

    public MailFactoryServiceTests()
    {
        _mailFactoryService = new MailFactoryService();
    }
    
    [Fact]
    public void CreateVerificationMail_WhenCalled_ReturnsMailData()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string verificationToken = "test verification token";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
        };
        
        var result = _mailFactoryService.CreateVerificationMail(user, verificationToken);
        
        Assert.NotNull(result);
        Assert.Equal(userEmail, result.EmailToId);
        Assert.Equal(userName, result.EmailToName);
    }
    
    [Fact]
    public void CreatePasswordResetMail_WhenCalled_ReturnsMailData()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string passwordResetToken = "test verification token";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
        };
        
        var result = _mailFactoryService.CreatePasswordResetMail(user, passwordResetToken);
        
        Assert.NotNull(result);
        Assert.Equal(userEmail, result.EmailToId);
        Assert.Equal(userName, result.EmailToName);
    }
    
    [Fact]
    public void CreatePasswordChangedMail_WhenCalled_ReturnsMailData()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
        };
        
        var result = _mailFactoryService.CreatePasswordChangedMail(user);
        
        Assert.NotNull(result);
        Assert.Equal(userEmail, result.EmailToId);
        Assert.Equal(userName, result.EmailToName);
    }
}