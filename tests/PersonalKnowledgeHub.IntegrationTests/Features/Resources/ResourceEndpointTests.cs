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
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Features.Resources;

[Collection(nameof(IntegrationCollection))]
public class ResourceEndpointTests : IntegrationTestBase
{
    public ResourceEndpointTests(IntegrationFixture fixture) : base(fixture) {}

    [Fact]
    public async Task GetResources_WhenRequestIsValid_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource1 = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource1);
        await dbContext.SaveChangesAsync();
        
        Resource resource2 = new Resource
        {
            Title = "english",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource2);
        await dbContext.SaveChangesAsync();
        
        Resource resource3 = new Resource
        {
            Title = "literature",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource3);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/resources?pageIndex=1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var body = await response.Content.ReadFromJsonAsync<PageResult<ResourceResponseDto>>(jsonOptions);
        
        Assert.NotNull(body);
        Assert.False(body.HasNextPage);
        Assert.False(body.HasPreviousPage);
        Assert.Contains(body.Items, r => r.Title == "math");
        Assert.Contains(body.Items, r => r.Title == "english");
        Assert.Contains(body.Items, r => r.Title == "literature");
    }

    [Fact]
    public async Task GetResources_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource1 = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource1);
        await dbContext.SaveChangesAsync();
        
        Resource resource2 = new Resource
        {
            Title = "english",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource2);
        await dbContext.SaveChangesAsync();
        
        Resource resource3 = new Resource
        {
            Title = "literature",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource3);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/resources?pageIndex=-1&pageSize=0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetResources_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource1 = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource1);
        await dbContext.SaveChangesAsync();
        
        Resource resource2 = new Resource
        {
            Title = "english",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource2);
        await dbContext.SaveChangesAsync();
        
        Resource resource3 = new Resource
        {
            Title = "literature",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource3);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/resources?pageIndex=1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetResourceById_WhenUserIsActive_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var body = await response.Content.ReadFromJsonAsync<ResourceResponseDto>(jsonOptions);
        
        Assert.NotNull(body);
        Assert.Equal(resource.Title, body.Title);
        Assert.Equal(resource.ResourceType, body.ResourceType);
    }

    [Fact]
    public async Task GetResourceById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetResourceById_WhenResourceDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/resources/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetResourceById_WhenUserDoesNotOwnResource_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "anotheruser@gmail.com",
            PasswordHash = "another user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(anotherUser);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = anotherUser.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddResource_WhenRequestIsValid_ReturnsCreated()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/resources");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceRequestDto
        {
            Title = "math",
            ResourceType = ResourceType.Book
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var body = await response.Content.ReadFromJsonAsync<ResourceResponseDto>(jsonOptions);
        
        Assert.NotNull(body);
        Assert.Equal("math", body.Title);
        Assert.Equal(ResourceType.Book, body.ResourceType);
        
        Resource? addedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Title == "math");
        
        Assert.NotNull(addedResource);
    }

    [Fact]
    public async Task AddResource_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/resources");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceRequestDto
        {
            Title = "",
            ResourceType = ResourceType.Book
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        Resource? addedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Title == "");
        
        Assert.Null(addedResource);
    }

    [Fact]
    public async Task AddResource_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/resources");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceRequestDto
        {
            Title = "math",
            ResourceType = ResourceType.Book
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Resource? addedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Title == "math");
        
        Assert.Null(addedResource);
    }

    [Fact]
    public async Task AddResource_WhenResourceAlreadyExists_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/resources");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceRequestDto
        {
            Title = "math",
            ResourceType = ResourceType.Book
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        List<Resource> addedResource = await dbContext.Resources.AsNoTracking().Where(r => r.Title == "math").ToListAsync();
        
        Assert.Single(addedResource);
    }

    [Fact]
    public async Task UpdateResourceById_WhenRequestIsValid_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceUpdateRequestDto
        {
            Title = "english"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.Equal("english", updatedResource.Title);
    }

    [Fact]
    public async Task UpdateResourceById_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceUpdateRequestDto
        {
            Title = ""
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.Equal("math", updatedResource.Title);
    }

    [Fact]
    public async Task UpdateResourceById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceUpdateRequestDto
        {
            Title = "english"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.Equal("math", updatedResource.Title);
    }

    [Fact]
    public async Task UpdateResourceById_WhenResourceDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"/resources/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceUpdateRequestDto
        {
            Title = "english"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateResourceById_WhenUserDoesNotOwnResource_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "anotheruser@gmail.com",
            PasswordHash = "another user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(anotherUser);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = anotherUser.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceUpdateRequestDto
        {
            Title = "english"
        });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.Equal("math", updatedResource.Title);
    }

    [Fact]
    public async Task DeleteResourceById_WhenUserIsActive_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.True(updatedResource.IsDeleted);
        Assert.NotNull(updatedResource.DeletedAt);
        Assert.NotNull(updatedResource.DeletedBy);
        Assert.Equal(user.Id, updatedResource.DeletedBy);
    }

    [Fact]
    public async Task DeleteResourceById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Inactive,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = user.Id,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.False(updatedResource.IsDeleted);
        Assert.Null(updatedResource.DeletedAt);
        Assert.Null(updatedResource.DeletedBy);
    }

    [Fact]
    public async Task DeleteResourceById_WhenResourceDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteResourceById_WhenUserDoesNotOwnResource_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active,
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "anotheruser@gmail.com",
            PasswordHash = "another user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(anotherUser);
        await dbContext.SaveChangesAsync();

        Resource resource = new Resource
        {
            Title = "math",
            ResourceType = ResourceType.Book,
            UserId = anotherUser.Id,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Resource? updatedResource = await dbContext.Resources
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(r => r.Id == resource.Id);
        
        Assert.NotNull(updatedResource);
        Assert.False(updatedResource.IsDeleted);
        Assert.Null(updatedResource.DeletedAt);
        Assert.Null(updatedResource.DeletedBy);
    }
}