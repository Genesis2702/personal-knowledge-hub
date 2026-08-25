using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Features.Auth;

[Collection(nameof(IntegrationCollection))]
public class AuthEndpointTests : IntegrationTestBase
{
    public AuthEndpointTests(IntegrationFixture fixture) : base(fixture) {}

    [Fact]
    public async Task Register_WhenEmailIsValid_ReturnsCreated()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/register");
        request.Content = JsonContent.Create(new RegisterRequestDto
        {
            UserName = "username",
            Email = "user@gmail.com",
            Password = "user password"
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(jobClient.Jobs);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        Assert.NotNull(body);
        Assert.NotNull(body.AccessToken);
        Assert.NotNull(body.RefreshToken);
        
        var job = jobClient.Jobs.Single();
        
        Assert.Equal(typeof(IMailService), job.Type);
        Assert.Equal(nameof(IMailService.SendMail), job.Method.Name);
        
        User? registeredUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == "username");
        
        Assert.NotNull(registeredUser);
    }
    
    [Fact]
    public async Task Register_WhenEmailIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/register");
        request.Content = JsonContent.Create(new RegisterRequestDto
        {
            UserName = "username",
            Email = "user@yahoo.com",
            Password = "user password"
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? registeredUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == "username");
        
        Assert.Null(registeredUser);
    }
    
    [Fact]
    public async Task Register_WhenEmailAlreadyExist_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            UserName = "username",
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/register");
        request.Content = JsonContent.Create(new RegisterRequestDto
        {
            UserName = "username",
            Email = "user@gmail.com",
            Password = "user password"
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        List<User> registeredUser = await dbContext.Users.AsNoTracking().Where(u => u.UserName == "username").ToListAsync();
        
        Assert.Single(registeredUser);
    }

    [Fact]
    public async Task Login_WhenCredentialsIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/login");
        request.Content = JsonContent.Create(new LoginRequestDto
        {
            Email = "user@gmail.com",
            Password = "user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        Assert.NotNull(body);
        Assert.NotNull(body.AccessToken);
        Assert.NotNull(body.RefreshToken);
    }

    [Fact]
    public async Task Login_WhenEmailIsWrong_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/login");
        request.Content = JsonContent.Create(new LoginRequestDto
        {
            Email = "wronguser@gmail.com",
            Password = "user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/login");
        request.Content = JsonContent.Create(new LoginRequestDto
        {
            Email = "user@gmail.com",
            Password = "wrong user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenUserIsLocked_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active,
            LockedUntil = DateTime.UtcNow.AddDays(1)
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/login");
        request.Content = JsonContent.Create(new LoginRequestDto
        {
            Email = "user@gmail.com",
            Password = "user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string refreshToken = await tokenService.GenerateRefreshToken(user.Id, Guid.NewGuid(), CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
        request.Content = JsonContent.Create(new RefreshRequestDto
        {
            RefreshToken = refreshToken
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        Assert.NotNull(body);
        Assert.NotNull(body.AccessToken);
        Assert.NotNull(body.RefreshToken);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        RefreshToken? revokedRefreshToken = await dbContext.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(revokedRefreshToken);
        Assert.True(revokedRefreshToken.Revoked);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
        request.Content = JsonContent.Create(new RefreshRequestDto
        {
            RefreshToken = "random refresh token"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsRevoked_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string refreshToken = await tokenService.GenerateRefreshToken(user.Id, Guid.NewGuid(), CancellationToken.None);
        
        RefreshToken addedRefreshToken = await tokenService.GetRefreshToken(refreshToken, CancellationToken.None);
        
        addedRefreshToken.Revoked = true;
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
        request.Content = JsonContent.Create(new RefreshRequestDto
        {
            RefreshToken = refreshToken
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        RefreshToken? revokedRefreshToken = await dbContext.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(revokedRefreshToken);
        Assert.True(revokedRefreshToken.Revoked);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsExpired_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string refreshToken = await tokenService.GenerateRefreshToken(user.Id, Guid.NewGuid(), CancellationToken.None);
        
        RefreshToken addedRefreshToken = await tokenService.GetRefreshToken(refreshToken, CancellationToken.None);
        
        addedRefreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
        request.Content = JsonContent.Create(new RefreshRequestDto
        {
            RefreshToken = refreshToken
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        RefreshToken? revokedRefreshToken = await dbContext.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(revokedRefreshToken);
        Assert.True(revokedRefreshToken.Revoked);
    }

    [Fact]
    public async Task Logout_WhenUserOwnsRefreshToken_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string refreshToken = await tokenService.GenerateRefreshToken(user.Id, Guid.NewGuid(), CancellationToken.None);
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new LogoutRequestDto
        {
            RefreshToken = refreshToken
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        RefreshToken? revokedRefreshToken = await dbContext.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(revokedRefreshToken);
        Assert.True(revokedRefreshToken.Revoked);
    }

    [Fact]
    public async Task Logout_WhenUserDoesNotOwnRefreshToken_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "anotheruser@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("another user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(anotherUser);
        await dbContext.SaveChangesAsync();
        
        string refreshToken = await tokenService.GenerateRefreshToken(anotherUser.Id, Guid.NewGuid(), CancellationToken.None);
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new LogoutRequestDto
        {
            RefreshToken = refreshToken
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        RefreshToken? revokedRefreshToken = await dbContext.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(revokedRefreshToken);
        Assert.False(revokedRefreshToken.Revoked);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailIsCorrect_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/forgot-password");
        request.Content = JsonContent.Create(new ForgotPasswordRequest
        {
            Email = user.Email
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(jobClient.Jobs);
        
        var job =  jobClient.Jobs.Single();
        
        Assert.Equal(typeof(IMailService), job.Type);
        Assert.Equal(nameof(IMailService.SendMail), job.Method.Name);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailIsNotCorrect_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/forgot-password");
        request.Content = JsonContent.Create(new ForgotPasswordRequest
        {
            Email = "wrong@gmail.com"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
    }

    [Fact]
    public async Task ChangePassword_WhenRequestIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/change-password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "new user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(jobClient.Jobs);
        
        var job = jobClient.Jobs.Single();
        
        Assert.Equal(typeof(IMailService), job.Type);
        Assert.Equal(nameof(IMailService.SendMail), job.Method.Name);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("new user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ChangePassword_WhenRequestIsInvalid_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/change-password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "wrong user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ChangePassword_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "auth/change-password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "new user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_WhenRequestIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/reset-password?token={verificationToken}");
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "new user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(jobClient.Jobs);
        
        var job = jobClient.Jobs.Single();
        
        Assert.Equal(typeof(IMailService), job.Type);
        Assert.Equal(nameof(IMailService.SendMail), job.Method.Name);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("new user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_WhenRequestIsInvalid_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/reset-password?token={verificationToken}");
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "wrong user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_WhenVerificationTokenDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/reset-password?token=random");
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "new user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_WhenVerificationTokenIsExpired_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.SingleOrDefaultAsync(t => t.Token == hashedToken);
        token!.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/reset-password?token={verificationToken}");
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "new user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_WhenVerificationTokenIsVerified_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.SingleOrDefaultAsync(t => t.Token == hashedToken);
        token!.VerifiedAt = DateTime.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync();
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/reset-password?token={verificationToken}");
        request.Content = JsonContent.Create(new ResetPasswordRequestDto
        {
            NewPassword = "new user password",
            ConfirmationPassword = "new user password"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
        
        User? passwordUpdatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(passwordUpdatedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("user password", passwordUpdatedUser.PasswordHash));
    }

    [Fact]
    public async Task VerifyMail_WhenVerificationTokenIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/mail-verification?token={verificationToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(token);
        Assert.NotNull(token.VerifiedAt);
        
        User? statusChangedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(statusChangedUser);
        Assert.Equal(UserStatus.Active, statusChangedUser.Status);
    }

    [Fact]
    public async Task VerifyMail_WhenVerificationTokenDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/mail-verification?token=random");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        User? statusChangedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(statusChangedUser);
        Assert.Equal(UserStatus.Pending, statusChangedUser.Status);
    }

    [Fact]
    public async Task VerifyMail_WhenVerificationTokenIsExpired_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.SingleOrDefaultAsync(t => t.Token == hashedToken);
        token!.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/mail-verification?token={verificationToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        User? statusChangedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(statusChangedUser);
        Assert.Equal(UserStatus.Pending, statusChangedUser.Status);
    }

    [Fact]
    public async Task VerifyMail_WhenUserDoesNotOwnVerificationToken_ReturnsUnauthorized()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "anotheruser@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("another user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(anotherUser);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(anotherUser.Id, CancellationToken.None);
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/mail-verification?token={verificationToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(token);
        Assert.Null(token.VerifiedAt);
        
        User? statusChangedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(statusChangedUser);
        Assert.Equal(UserStatus.Pending, statusChangedUser.Status);
    }

    [Fact]
    public async Task VerifyMail_WhenVerificationTokenIsVerified_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.SingleOrDefaultAsync(t => t.Token == hashedToken);
        token!.VerifiedAt = DateTime.UtcNow.AddDays(-1);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/mail-verification?token={verificationToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        User? statusChangedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(statusChangedUser);
        Assert.Equal(UserStatus.Pending, statusChangedUser.Status);
    }

    [Fact]
    public async Task VerifyMail_WhenUserIsNotPending_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var verificationTokenService =  scope.ServiceProvider.GetRequiredService<IVerificationTokenService>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string verificationToken = await verificationTokenService.GenerateVerificationToken(user.Id, CancellationToken.None);
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"auth/mail-verification?token={verificationToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(verificationToken)));
        VerificationToken? token = await dbContext.VerificationTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Token == hashedToken);
        
        Assert.NotNull(token);
        Assert.Null(token.VerifiedAt);
        
        User? statusChangedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(t => t.Id == user.Id);
        
        Assert.NotNull(statusChangedUser);
        Assert.Equal(UserStatus.Active, statusChangedUser.Status);
    }

    [Fact]
    public async Task ResendMail_WhenUserIsPending_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Pending
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/auth/resend-verification");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(jobClient.Jobs);
        
        var job =  jobClient.Jobs.Single();
        
        Assert.NotNull(job);
        Assert.Equal(typeof(IMailService), job.Type);
        Assert.Equal(nameof(IMailService.SendMail), job.Method.Name);
    }

    [Fact]
    public async Task ResendMail_WhenUserIsNotPending_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var jobClient = scope.ServiceProvider.GetRequiredService<RecordingBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user password"),
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/auth/resend-verification");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(jobClient.Jobs);
    }
}