using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Features.Roles;

[Collection(nameof(IntegrationCollection))]
public class RoleEndpointTests : IntegrationTestBase
{
    public RoleEndpointTests(IntegrationFixture fixture) : base(fixture) {}

    [Fact]
    public async Task GetRoles_WhenUserIsAdmin_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<RoleResponseDto>>();
        
        Assert.NotNull(body);
        Assert.Contains(body, role => role.Name == "ADMIN");
        Assert.Contains(body, role => role.Name == "USER");
    }

    [Fact]
    public async Task GetRoles_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRoleById_WhenUserIsAdmin_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RoleResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal(adminRole.Name, body.Name);
    }

    [Fact]
    public async Task GetRoleById_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRoleById_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/roles/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddRole_WhenRequestIsValid_ReturnsCreated()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "USER"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<RoleResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal("USER", body.Name);
        
        Role? addedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Name == "USER");
        
        Assert.NotNull(addedRole);
    }

    [Fact]
    public async Task AddRole_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = ""
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        Role? addedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Name == "");
        
        Assert.Null(addedRole);
    }

    [Fact]
    public async Task AddRole_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "USER"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? addedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Name == "USER");
        
        Assert.Null(addedRole);
    }

    [Fact]
    public async Task AddRole_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "USER"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? addedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Name == "USER");
        
        Assert.Null(addedRole);
    }

    [Fact]
    public async Task UpdateRoleById_WhenRequestIsValid_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "CLIENT"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Role? updatedRole = await  dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id ==  userRole.Id);
        
        Assert.NotNull(updatedRole);
        Assert.Equal("CLIENT", updatedRole.Name);
    }

    [Fact]
    public async Task UpdateRoleById_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = ""
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        Role? updatedRole = await  dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id ==  userRole.Id);
        
        Assert.NotNull(updatedRole);
        Assert.Equal("USER", updatedRole.Name);
    }

    [Fact]
    public async Task UpdateRoleById_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "CLIENT"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? updatedRole = await  dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id ==  userRole.Id);
        
        Assert.NotNull(updatedRole);
        Assert.Equal("USER", updatedRole.Name);
    }

    [Fact]
    public async Task UpdateRoleById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "CLIENT"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? updatedRole = await  dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id ==  userRole.Id);
        
        Assert.NotNull(updatedRole);
        Assert.Equal("USER", updatedRole.Name);
    }

    [Fact]
    public async Task UpdateRoleById_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/roles/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "CLIENT"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRoleById_WhenRoleAlreadyExists_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new RoleRequestDto
        {
            Name = "USER"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        Role? updatedRole = await  dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id ==  userRole.Id);
        
        Assert.NotNull(updatedRole);
        Assert.Equal("USER", updatedRole.Name);
    }

    [Fact]
    public async Task DeleteRoleById_WhenUserIsAdmin_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Role? deletedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id == userRole.Id);
        
        Assert.Null(deletedRole);
    }

    [Fact]
    public async Task DeleteRoleById_WhenUserIsNotAdmin_ReturnForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? deletedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id == userRole.Id);
        
        Assert.NotNull(deletedRole);
    }

    [Fact]
    public async Task DeleteRoleById_WhenUserIsNotActive_ReturnForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Role userRole = new Role
        {
            Name = "USER"
        };
        
        dbContext.Roles.Add(userRole);
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{userRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? deletedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id == userRole.Id);
        
        Assert.NotNull(deletedRole);
    }

    [Fact]
    public async Task DeleteRoleById_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRoleById_WhenRoleIsAdmin_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{adminRole.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        Role? deletedRole = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(deletedRole);
    }

    [Fact]
    public async Task AddPermissionToRole_WhenUserIsAdmin_ReturnsCreated()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles/{adminRole.Id}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        Role? addedPermissionRole =  await dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(addedPermissionRole);
        Assert.Contains(addedPermissionRole.RolePermissions, rp => rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task AddPermissionToRole_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles/{adminRole.Id}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? addedPermissionRole =  await dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(addedPermissionRole);
        Assert.DoesNotContain(addedPermissionRole.RolePermissions, rp => rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task AddPermissionToRole_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive
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

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles/{adminRole.Id}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? addedPermissionRole =  await dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(addedPermissionRole);
        Assert.DoesNotContain(addedPermissionRole.RolePermissions, rp => rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task AddPermissionToRole_WhenRoleDoesNotExist_ReturnsNotFound()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles/{Int16.MaxValue}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPermissionToRole_WhenPermissionDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/roles/{adminRole.Id}/permissions/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        Role? addedPermissionRole =  await dbContext.Roles.AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(addedPermissionRole);
        Assert.Empty(addedPermissionRole.RolePermissions);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenUserIsAdmin_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        RolePermission rolePermission = new RolePermission
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            PermissionId = permission.Id,
            Permission = permission
        };
        
        dbContext.RolePermissions.Add(rolePermission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{adminRole.Id}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Role? deletedPermissionRole = await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(deletedPermissionRole);
        Assert.Empty(deletedPermissionRole.RolePermissions);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenUserIsNotAdmin_ReturnsForbidden()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();

        Role adminRole = new Role
        {
            Name = "ADMIN"
        };
        
        dbContext.Roles.Add(adminRole);
        await dbContext.SaveChangesAsync();

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        RolePermission rolePermission = new RolePermission
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            PermissionId = permission.Id,
            Permission = permission
        };
        
        dbContext.RolePermissions.Add(rolePermission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{adminRole.Id}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? deletedPermissionRole = await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(deletedPermissionRole);
        Assert.Contains(deletedPermissionRole.RolePermissions, rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenUserIsNotActive_ReturnsForbidden()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Inactive
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

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        RolePermission rolePermission = new RolePermission
        {
            RoleId = adminRole.Id,
            Role = adminRole,
            PermissionId = permission.Id,
            Permission = permission
        };
        
        dbContext.RolePermissions.Add(rolePermission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{adminRole.Id}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Role? deletedPermissionRole = await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(deletedPermissionRole);
        Assert.Contains(deletedPermissionRole.RolePermissions, rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenRoleDoesNotExist_ReturnsNotFound()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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

        Permission permission = new Permission
        {
            Name = "test permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{Int16.MaxValue}/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemovePermissionFromRole_WhenPermissionDoesNotExist_ReturnsNotFound()
    {
                await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User admin = new User
        {
            Email = "admin@gmail.com",
            PasswordHash = "admin password",
            Status = UserStatus.Active
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/roles/{adminRole.Id}/permissions/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        Role? deletedPermissionRole = await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SingleOrDefaultAsync(role => role.Id == adminRole.Id);
        
        Assert.NotNull(deletedPermissionRole);
        Assert.Empty(deletedPermissionRole.RolePermissions);
    }
}