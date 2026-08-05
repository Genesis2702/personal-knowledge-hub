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

namespace PersonalKnowledgeHub.IntegrationTests.Features.Tags;

[Collection(nameof(IntegrationCollection))]
public sealed class TagEndpointTests : IntegrationTestBase
{
    public TagEndpointTests(IntegrationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddTag_WhenRequestIsValid_ReturnsCreated()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<TagResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal("test tag", body.Name);
        Assert.NotNull(response.Headers.Location);

        Tag? savedTag = await dbContext.Tags.SingleOrDefaultAsync(tag =>
            tag.UserId == user.Id &&
            tag.Name == "test tag");

        Assert.NotNull(savedTag);
    }

    [Fact]
    public async Task AddTag_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = ""
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        List<Tag> tags = await dbContext.Tags.Where(tag => tag.UserId == user.Id).ToListAsync();
        
        Assert.Empty(tags);
    }
    
    [Fact]
    public async Task AddTag_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Inactive,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Tag? savedTag = await dbContext.Tags.SingleOrDefaultAsync(tag =>
            tag.UserId == user.Id &&
            tag.Name == "test tag");

        Assert.Null(savedTag);
    }

    [Fact]
    public async Task AddTag_WhenTagAlreadyExists_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag duplicatedTag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(duplicatedTag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        List<Tag> savedTag = await dbContext.Tags.Where(tag =>
            tag.UserId == user.Id &&
            tag.Name == "test tag").ToListAsync();
        
        Assert.Single(savedTag);
    }

    [Fact]
    public async Task GetTags_WhenUserIsActive_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await  dbContext.SaveChangesAsync();

        Tag firstTag = new Tag
        {
            Name = "first tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(firstTag);
        await dbContext.SaveChangesAsync();

        Tag secondTag = new Tag
        {
            Name = "second tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(secondTag);
        await dbContext.SaveChangesAsync();

        Tag thirdTag = new Tag
        {
            Name = "third tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(thirdTag);
        await  dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<List<TagResponseDto>>();

        Assert.NotNull(body);
        Assert.Equal(3, body.Count);
        Assert.Contains(body, tag => tag.Name == firstTag.Name);
        Assert.Contains(body, tag => tag.Name == secondTag.Name);
        Assert.Contains(body, tag => tag.Name == thirdTag.Name);
    }

    [Fact]
    public async Task GetTags_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Inactive,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await  dbContext.SaveChangesAsync();

        Tag firstTag = new Tag
        {
            Name = "first tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(firstTag);
        await dbContext.SaveChangesAsync();

        Tag secondTag = new Tag
        {
            Name = "second tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(secondTag);
        await dbContext.SaveChangesAsync();

        Tag thirdTag = new Tag
        {
            Name = "third tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(thirdTag);
        await  dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/tags");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTagById_WhenUserIsActive_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var  tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await  dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body =  await response.Content.ReadFromJsonAsync<TagResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal(tag.Name, body.Name);
    }

    [Fact]
    public async Task GetTagById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var  tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Inactive,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await  dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTagById_WhenTagDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var  tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await  dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/tags/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTagById_WhenUserDoesNotOwnTag_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var  tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User currentUser = new User
        {
            Email = "test1@gmail.com",
            PasswordHash = "test password1",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(currentUser);
        await  dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "test2@gmail.com",
            PasswordHash = "test password2",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(anotherUser);
        await  dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = anotherUser.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(currentUser.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTagById_WhenRequestIsValid_ReturnsNoContent() 
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "updated test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Tag? savedTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(savedTag);
        Assert.Equal("updated test tag", savedTag.Name);
        Assert.Equal(user.Id, savedTag.UserId);
    }

    [Fact]
    public async Task UpdateTagById_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = ""
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        Tag? savedTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(savedTag);
        Assert.Equal("test tag", savedTag.Name);
        Assert.Equal(user.Id, savedTag.UserId);
    }

    [Fact]
    public async Task UpdateTagById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Inactive,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "updated test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Tag? savedTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(savedTag);
        Assert.Equal("test tag", savedTag.Name);
        Assert.Equal(user.Id, savedTag.UserId);
    }

    [Fact]
    public async Task UpdateTagById_WhenTagDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/tags/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "updated test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        Tag? savedTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t =>
            t.UserId == user.Id &&
            t.Name == "updated test tag");
        
        Assert.Null(savedTag);
    }

    [Fact]
    public async Task UpdateTagById_WhenUserDoesNotOwnTag_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        User currentUser = new User
        {
            Email = "test1@gmail.com",
            PasswordHash = "test password1",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        User otherUser = new User
        {
            Email = "test2@gmail.com",
            PasswordHash = "test password2",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(otherUser);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = otherUser.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(currentUser.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "updated test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Tag? savedTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(savedTag);
        Assert.Equal("test tag", savedTag.Name);
        Assert.Equal(otherUser.Id, savedTag.UserId);
    }

    [Fact]
    public async Task UpdateTagById_WhenTagAlreadyExists_ReturnsConflict()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag originalTag = new Tag
        {
            Name = "test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(originalTag);
        await dbContext.SaveChangesAsync();

        Tag anotherTag = new Tag
        {
            Name = "another test tag",
            UserId = user.Id
        };
        
        dbContext.Tags.Add(anotherTag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"/tags/{originalTag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(
            new TagRequestDto
            {
                Name = "another test tag"
            });
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        Tag? savedOriginalTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == originalTag.Id);
        
        Assert.NotNull(savedOriginalTag);
        Assert.Equal(originalTag.Name, savedOriginalTag.Name);
        Assert.Equal(originalTag.UserId, savedOriginalTag.UserId);
        
        Tag? savedAnotherTag = await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == anotherTag.Id);
        
        Assert.NotNull(savedAnotherTag);
        Assert.Equal(anotherTag.Name, savedAnotherTag.Name);
        Assert.Equal(anotherTag.UserId, savedAnotherTag.UserId);
    }

    [Fact]
    public async Task DeleteTagById_WhenUserIsActive_ReturnsNoContent()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null,
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        Tag? seededTag =  await dbContext.Tags.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);

        Assert.NotNull(seededTag);
        Assert.True(seededTag.IsDeleted);
        Assert.NotNull(seededTag.DeletedAt);
        Assert.Equal(user.Id, seededTag.DeletedBy);
    }

    [Fact]
    public async Task DeleteTagById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Inactive,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Tag? seededTag =  await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(seededTag);
        Assert.False(seededTag.IsDeleted);
        Assert.Null(seededTag.DeletedAt);
        Assert.Null(seededTag.DeletedBy);
    }

    [Fact]
    public async Task DeleteTagById_WhenTagDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/tags/{Int16.MaxValue}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTagById_WhenUserDoesNotOwnTag_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User currentUser = new User
        {
            Email = "test1@gmail.com",
            PasswordHash = "test password1",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "test2@gmail.com",
            PasswordHash = "test password2",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(anotherUser);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = anotherUser.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(currentUser.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"/tags/{tag.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Tag? seededTag =  await dbContext.Tags.AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(seededTag);
        Assert.False(seededTag.IsDeleted);
        Assert.Null(seededTag.DeletedAt);
        Assert.Null(seededTag.DeletedBy);
    }

    [Fact]
    public async Task RestoreTagById_WhenUserIsActive_ReturnsOk()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/tags/{tag.Id}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<TagResponseDto>();
        
        Assert.NotNull(body);
        Assert.Equal(tag.Name,  body.Name);
        
        Tag? seededTag = await dbContext.Tags.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(seededTag);
        Assert.False(seededTag.IsDeleted);
        Assert.Null(seededTag.DeletedAt);
        Assert.Null(seededTag.DeletedBy);
    }

    [Fact]
    public async Task RestoreTagById_WhenUserIsNotActive_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Inactive,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = user.Id,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = user.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/tags/{tag.Id}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Tag? seededTag = await dbContext.Tags.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(seededTag);
        Assert.True(seededTag.IsDeleted);
        Assert.NotNull(seededTag.DeletedAt);
        Assert.Equal(user.Id, seededTag.DeletedBy);
    }

    [Fact]
    public async Task RestoreTagById_WhenTagDoesNotExist_ReturnsNotFound()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User user = new User
        {
            Email = "test@gmail.com",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(user.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/tags/{Int16.MaxValue}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreTagById_WhenUserDoesNotOwnTag_ReturnsForbidden()
    {
        await using var scope = Fixture.Factory!.Services.CreateAsyncScope();
        var dbContext =  scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        User currentUser = new User
        {
            Email = "test1@gmail.com",
            PasswordHash = "test password1",
            Status = UserStatus.Active,
            Tags = []
        };
        
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        User anotherUser = new User
        {
            Email = "test2@gmail.com",
            PasswordHash = "test password2",
            Status = UserStatus.Active,
            Tags = []
        };

        dbContext.Add(anotherUser);
        await dbContext.SaveChangesAsync();

        Tag tag = new Tag
        {
            Name = "test tag",
            UserId = anotherUser.Id,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = anotherUser.Id
        };
        
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        
        string accessToken = await tokenService.GenerateAccessToken(currentUser.Id, CancellationToken.None);
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"/tags/{tag.Id}/restore");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await Fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        Tag? seededTag = await dbContext.Tags.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(t => t.Id == tag.Id);
        
        Assert.NotNull(seededTag);
        Assert.True(seededTag.IsDeleted);
        Assert.NotNull(seededTag.DeletedAt);
        Assert.Equal(anotherUser.Id, seededTag.DeletedBy);
    }
}