using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Features.Permissions;

[Collection(nameof(IntegrationCollection))]
public class PermissionEndpointTests : IntegrationTestBase
{
    public  PermissionEndpointTests(IntegrationFixture fixture) : base(fixture) {}

    [Fact]
    public async Task GetPermissions_WhenUserIsAdmin_ReturnsOk()
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

        Permission permission1 = new Permission
        {
            Name = "Permission1"
        };
        
        dbContext.Permissions.Add(permission1);
        await dbContext.SaveChangesAsync();

        Permission permission2 = new Permission
        {
            Name = "Permission2"
        };
        
        dbContext.Permissions.Add(permission2);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<PermissionResponseDto>>();
        
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
        Assert.Contains(body, p => p.Name == permission1.Name);
        Assert.Contains(body, p => p.Name == permission2.Name);
    }

    [Fact]
    public async Task GetPermissions_WhenUserIsNotAdmin_ReturnsForbidden()
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

        Permission permission1 = new Permission
        {
            Name = "Permission1"
        };
        
        dbContext.Permissions.Add(permission1);
        await dbContext.SaveChangesAsync();

        Permission permission2 = new Permission
        {
            Name = "Permission2"
        };
        
        dbContext.Permissions.Add(permission2);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPermissions_WhenUserIsNotActive_ReturnsForbidden()
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

        Permission permission1 = new Permission
        {
            Name = "Permission1"
        };
        
        dbContext.Permissions.Add(permission1);
        await dbContext.SaveChangesAsync();

        Permission permission2 = new Permission
        {
            Name = "Permission2"
        };
        
        dbContext.Permissions.Add(permission2);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPermissionById_WhenUserIsAdmin_ReturnsOk()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PermissionResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal("permission", body.Name);
    }

    [Fact]
    public async Task GetPermissionById_WhenUserIsNotAdmin_ReturnsForbidden()
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

        Permission permission = new Permission
        {
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPermissionById_WhenUserIsNotActive_ReturnsForbidden()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPermissionById_WhenPermissionDoesNotExist_ReturnsNotFound()
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/permissions/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPermission_WhenRequestIsValid_ReturnsCreated()
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await  response.Content.ReadFromJsonAsync<PermissionResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal("permission", body.Name);
        
        Permission? addedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Name == "permission");
        
        Assert.NotNull(addedPermission);
    }

    [Fact]
    public async Task AddPermission_WhenRequestIsInvalid_ReturnsBadRequest()
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = ""
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        Permission? addedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Name == "");
        
        Assert.Null(addedPermission);
    }

    [Fact]
    public async Task AddPermission_WhenUserIsNotAdmin_ReturnsForbidden()
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
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Permission? addedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Name == "permission");
        
        Assert.Null(addedPermission);
    }

    [Fact]
    public async Task AddPermission_WhenUserIsNotActive_ReturnsForbidden()
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
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Permission? addedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Name == "permission");
        
        Assert.Null(addedPermission);
    }

    [Fact]
    public async Task AddPermission_WhenPermissionAlreadyExists_ReturnsConflict()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        List<Permission> addedPermission = await dbContext.Permissions.AsNoTracking().Where(p => p.Name == "permission").ToListAsync();
        
        Assert.Single(addedPermission);
    }

    [Fact]
    public async Task UpdatePermissionById_WhenRequestIsValid_ReturnsNoContent()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "updated permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Permission? updatedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.NotNull(updatedPermission);
        Assert.Equal("updated permission", updatedPermission.Name);
    }

    [Fact]
    public async Task UpdatePermissionById_WhenRequestIsInvalid_ReturnsBadRequest()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = ""
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        Permission? updatedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.NotNull(updatedPermission);
        Assert.Equal("permission", updatedPermission.Name);
    }

    [Fact]
    public async Task UpdatePermissionById_WhenUserIsNotAdmin_ReturnsForbidden()
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

        Permission permission = new Permission
        {
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "updated permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Permission? updatedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.NotNull(updatedPermission);
        Assert.Equal("permission", updatedPermission.Name);
    }

    [Fact]
    public async Task UpdatePermissionById_WhenUserIsNotActive_ReturnsForbidden()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "updated permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Permission? updatedPermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.NotNull(updatedPermission);
        Assert.Equal("permission", updatedPermission.Name);
    }

    [Fact]
    public async Task UpdatePermissionById_WhenPermissionDoesNotExist_ReturnsNotFound()
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/permissions/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new PermissionRequestDto
        {
            Name = "updated permission"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePermissionById_WhenUserIsAdmin_ReturnsNoContent()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Permission? deletePermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.Null(deletePermission);
    }

    [Fact]
    public async Task DeletePermissionById_WhenUserIsNotAdmin_ReturnsForbidden()
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

        Permission permission = new Permission
        {
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Permission? deletePermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.NotNull(deletePermission);
    }

    [Fact]
    public async Task DeletePermissionById_WhenUserIsNotActive_ReturnsForbidden()
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
            Name = "permission"
        };
        
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(admin.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/permissions/{permission.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Permission? deletePermission = await dbContext.Permissions.AsNoTracking().SingleOrDefaultAsync(p => p.Id == permission.Id);
        
        Assert.NotNull(deletePermission);
    }

    [Fact]
    public async Task DeletePermissionById_WhenPermissionDoesNotExist_ReturnsNotFound()
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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/permissions/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}