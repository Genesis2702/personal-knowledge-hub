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

namespace PersonalKnowledgeHub.IntegrationTests.Features.ResourceTags;

[Collection(nameof(IntegrationCollection))]
public class ResourceTagEndpointTests : IntegrationTestBase
{
    public ResourceTagEndpointTests(IntegrationFixture fixture) : base(fixture) {}

    [Fact]
    public async Task AddResourceTag_WhenUserIsActive_ReturnsCreated()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/resources/{resource.Id}/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceTagRequestDto
        {
            TagId = tag.Id
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ResourceResponseDto>();
        
        Assert.NotNull(body);
        Assert.Contains(body.Tags, tagName => tagName == tag.Name);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.NotNull(addedResourceTag);
    }

    [Fact]
    public async Task AddResourceTag_WhenUserIsNotActive_ReturnsForbidden()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/resources/{resource.Id}/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceTagRequestDto
        {
            TagId = tag.Id
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.Null(addedResourceTag);
    }

    [Fact]
    public async Task AddResourceTag_WhenResourceDoesNotExist_ReturnsNotFound()
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

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/resources/{Int16.MaxValue}/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceTagRequestDto
        {
            TagId = tag.Id
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddResourceTag_WhenTagDoesNotExist_ReturnsNotFound()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/resources/{resource.Id}/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceTagRequestDto
        {
            TagId = Int16.MaxValue
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddResourceTag_WhenUserDoesNotOwnResource_ReturnsForbidden()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = anotherUser.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/resources/{resource.Id}/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceTagRequestDto
        {
            TagId = tag.Id
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.Null(addedResourceTag);
    }

    [Fact]
    public async Task AddResourceTag_WhenUserDoesNotOwnTag_ReturnsForbidden()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = anotherUser.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/resources/{resource.Id}/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ResourceTagRequestDto
        {
            TagId = tag.Id
        });

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.Null(addedResourceTag);
    }
    
    [Fact]
    public async Task DeleteResourceTag_WhenUserIsActive_ReturnsNoContent()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        ResourceTag resourceTag = new ResourceTag
        {
            ResourceId = resource.Id,
            Resource = resource,
            TagId = tag.Id,
            Tag = tag
        };
        
        dbContext.ResourceTags.Add(resourceTag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.Null(addedResourceTag);
    }

    [Fact]
    public async Task DeleteResourceTag_WhenUserIsNotActive_ReturnsForbidden()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        ResourceTag resourceTag = new ResourceTag
        {
            ResourceId = resource.Id,
            Resource = resource,
            TagId = tag.Id,
            Tag = tag
        };
        
        dbContext.ResourceTags.Add(resourceTag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.NotNull(addedResourceTag);
    }

    [Fact]
    public async Task DeleteResourceTag_WhenResourceTagDoesNotExist_ReturnsNotFound()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = user.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteResourceTag_WhenUserDoesNotOwnResourceTag_ReturnsForbidden()
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
            Title = "resource",
            ResourceType = ResourceType.Book,
            UserId = anotherUser.Id
        };
        
        dbContext.Resources.Add(resource);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "tag",
            UserId = anotherUser.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();

        ResourceTag resourceTag = new ResourceTag
        {
            ResourceId = resource.Id,
            Resource = resource,
            TagId = tag.Id,
            Tag = tag
        };
        
        dbContext.ResourceTags.Add(resourceTag);
        await dbContext.SaveChangesAsync();

        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/resources/{resource.Id}/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ResourceTag? addedResourceTag = await dbContext.ResourceTags.AsNoTracking().SingleOrDefaultAsync(rt => rt.ResourceId == resource.Id && rt.TagId == tag.Id);
        
        Assert.NotNull(addedResourceTag);
    }
}