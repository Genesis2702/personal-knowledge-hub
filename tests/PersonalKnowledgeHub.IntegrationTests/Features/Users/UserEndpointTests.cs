using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Features.Users;

[Collection(nameof(IntegrationCollection))]
public sealed class UserEndpointTests : IntegrationTestBase
{
    public UserEndpointTests(IntegrationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUsers_WhenRequestIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user1 = new User
        {
            Email = "user1@gmail.com",
            PasswordHash = "user1 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user1);
        await dbContext.SaveChangesAsync();

        User user2 = new User
        {
            Email = "user2@gmail.com",
            PasswordHash = "user2 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user2);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN",
            UserRoles = []
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            Role = adminRole,
            RoleId = adminRole.Id,
            User = admin,
            UserId = admin.Id
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var body =  await response.Content.ReadFromJsonAsync<PageResult<UserResponseDto>>(jsonOptions);
        
        Assert.NotNull(body);
        Assert.False(body.HasPreviousPage);
        Assert.False(body.HasNextPage);
        Assert.Equal(1, body.PageCount);
        Assert.Contains(body.Items, user => user.Email == "admin@gmail.com");
        Assert.Contains(body.Items, user => user.Email == "user1@gmail.com");
        Assert.Contains(body.Items, user => user.Email == "user2@gmail.com");
    }

    [Fact]
    public async Task GetUsers_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user1 = new User
        {
            Email = "user1@gmail.com",
            PasswordHash = "user1 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user1);
        await dbContext.SaveChangesAsync();

        User user2 = new User
        {
            Email = "user2@gmail.com",
            PasswordHash = "user2 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user2);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN",
            UserRoles = []
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            Role = adminRole,
            RoleId = adminRole.Id,
            User = admin,
            UserId = admin.Id
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/users?pageIndex=-1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WhenUserIsNotAdmin_ReturnsForbidden()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User normalUser = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(normalUser);
        await dbContext.SaveChangesAsync();

        User user1 = new User
        {
            Email = "user1@gmail.com",
            PasswordHash = "user1 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user1);
        await dbContext.SaveChangesAsync();

        User user2 = new User
        {
            Email = "user2@gmail.com",
            PasswordHash = "user2 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user2);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(normalUser.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WhenUserIsNotActive_ReturnsForbidden()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user1 = new User
        {
            Email = "user1@gmail.com",
            PasswordHash = "user1 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user1);
        await dbContext.SaveChangesAsync();

        User user2 = new User
        {
            Email = "user2@gmail.com",
            PasswordHash = "user2 password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user2);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN",
            UserRoles = []
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            Role = adminRole,
            RoleId = adminRole.Id,
            User = admin,
            UserId = admin.Id
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN",
            UserRoles = []
        };
        
        dbContext.Roles.Add(adminRole);
        await  dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            Role = adminRole,
            RoleId = adminRole.Id,
            User = admin,
            UserId = admin.Id
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/users/{user.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var body = await response.Content.ReadFromJsonAsync<UserResponseDto>(jsonOptions);
        
        Assert.NotNull(body);
        Assert.Equal(user.Email, body.Email);
    }

    [Fact]
    public async Task GetUserById_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN",
            UserRoles = []
        };
        
        dbContext.Roles.Add(adminRole);
        await  dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            Role = adminRole,
            RoleId = adminRole.Id,
            User = admin,
            UserId = admin.Id
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/users/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WhenUserIsNotAdmin_ReturnsForbidden()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/users/{user.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    
    [Fact]
    public async Task GetUserById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN",
            UserRoles = []
        };
        
        dbContext.Roles.Add(adminRole);
        await  dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            Role = adminRole,
            RoleId = adminRole.Id,
            User = admin,
            UserId = admin.Id
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/users/{user.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_WhenUserIsActive_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string  accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/users/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var body = await response.Content.ReadFromJsonAsync<UserResponseDto>(jsonOptions);
        
        Assert.NotNull(body);
        Assert.Equal(user.Email, body.Email);
    }

    [Fact]
    public async Task GetUserProfile_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string  accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/users/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenUserIsActive_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            UserName = "username",
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, "/users/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new UserUpdateRequestDto
        {
            UserName = "updated username"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        User? updatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(updatedUser);
        Assert.Equal("updated username", updatedUser.UserName);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            UserName = "username",
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, "/users/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new UserUpdateRequestDto
        {
            UserName = "updated username"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        User? updatedUser = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(updatedUser);
        Assert.Equal("username", updatedUser.UserName);
    }

    [Fact]
    public async Task BanUser_WhenUserIsAdmin_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            BannedAt = null
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/ban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        User? bannedUser =  await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(bannedUser);
        Assert.Equal(UserStatus.Banned,  bannedUser.Status);
        Assert.NotNull(bannedUser.BannedAt);
    }

    [Fact]
    public async Task BanUser_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            BannedAt = null
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/ban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        User? bannedUser =  await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(bannedUser);
        Assert.Equal(UserStatus.Active,  bannedUser.Status);
        Assert.Null(bannedUser.BannedAt);
    }

    [Fact]
    public async Task BanUser_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            BannedAt = null
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/ban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        User? bannedUser =  await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(bannedUser);
        Assert.Equal(UserStatus.Active,  bannedUser.Status);
        Assert.Null(bannedUser.BannedAt);
    }

    [Fact]
    public async Task BanUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
        
        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{Int16.MaxValue}/ban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnbanUser_WhenUserIsAdmin_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Banned,
            BannedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/unban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        User? unbannedUser =  await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(unbannedUser);
        Assert.Equal(UserStatus.Active, unbannedUser.Status);
    }

    [Fact]
    public async Task UnbanUser_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Banned,
            BannedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/unban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        User? unbannedUser =  await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(unbannedUser);
        Assert.Equal(UserStatus.Banned, unbannedUser.Status);
    }

    [Fact]
    public async Task UnbanUser_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Banned,
            BannedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/unban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        User? unbannedUser =  await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(unbannedUser);
        Assert.Equal(UserStatus.Banned, unbannedUser.Status);
    }

    [Fact]
    public async Task UnbanUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{Int16.MaxValue}/unban");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddRoleToUser_WhenUserIsAdmin_ReturnsCreated()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        User? roleAddedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleAddedUser);
        Assert.Contains(roleAddedUser.UserRoles, ur => ur.Role.Id == adminRole.Id);
    }

    [Fact]
    public async Task AddRoleToUser_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        User? roleAddedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleAddedUser);
        Assert.DoesNotContain(roleAddedUser.UserRoles, ur => ur.Role.Id == adminRole.Id);
    }

    [Fact]
    public async Task AddRoleToUser_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        User? roleAddedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleAddedUser);
        Assert.DoesNotContain(roleAddedUser.UserRoles, ur => ur.Role.Id == adminRole.Id);
    }

    [Fact]
    public async Task AddRoleToUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{Int16.MaxValue}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddRoleToUser_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/users/{user.Id}/roles/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        User? roleAddedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleAddedUser);
        Assert.Empty(roleAddedUser.UserRoles);
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenUserIsAdmin_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();

        UserRole userUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = user.Id,
            User = user
        };
        
        dbContext.UserRoles.Add(userUserRole);
        await  dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{user.Id}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        User? roleRemovedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleRemovedUser);
        Assert.Empty(roleRemovedUser.UserRoles);
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole userUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = user.Id,
            User = user
        };
        
        dbContext.UserRoles.Add(userUserRole);
        await  dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{user.Id}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        User? roleRemovedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleRemovedUser);
        Assert.Contains(roleRemovedUser.UserRoles, u => u.Role.Id == adminRole.Id);
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();

        UserRole userUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = user.Id,
            User = user
        };
        
        dbContext.UserRoles.Add(userUserRole);
        await  dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{user.Id}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        User? roleRemovedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleRemovedUser);
        Assert.Contains(roleRemovedUser.UserRoles, u => u.RoleId ==  adminRole.Id);
    }
    
    [Fact]
    public async Task RemoveRoleFromUser_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{Int16.MaxValue}/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
            UserRoles = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        UserRole adminUserRole = new UserRole
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            UserId = admin.Id,
            User = admin
        };
        
        dbContext.UserRoles.Add(adminUserRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{user.Id}/roles/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        User? roleRemovedUser = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(u => u.Id == user.Id);
        
        Assert.NotNull(roleRemovedUser);
        Assert.Empty(roleRemovedUser.UserRoles);
    }
}