using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Redis;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Platforms.Redis;

[Collection(nameof(RedisCollection))]
public class RedisTests
{
    private readonly RedisFixture _fixture;
    
    public RedisTests(RedisFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetResources_WhenCacheIsEmpty_CachesReturnedResources()
    {
        await using var scope = _fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

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
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/resources");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        HttpResponseMessage response = await _fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string cachedKey = $"resource:{user.Id}:1:10";
        string? cachedResources = await cache.GetStringAsync(cachedKey);
        
        Assert.NotNull(cachedResources);

        PageResult<Resource>? result = JsonSerializer.Deserialize<PageResult<Resource>>(cachedResources);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.PageIndex);
        Assert.Equal(10, result.PageSize);
        Assert.Contains(result.Items, r => r.Title == "math");
    }
}