using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.RateLimiting;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Platforms.RateLimiting;

[Collection(nameof(RateLimitingCollection))]
public class RateLimitingTests
{
    private readonly RateLimitingFixture _fixture;

    public RateLimitingTests(RateLimitingFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetTags_WhenAbuseEndpoint_ReturnsTooManyRequests()
    {
        await using var scope = _fixture.Factory!.Services.CreateAsyncScope();
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

        for (int i = 0; i < 10; i++)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/tags");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            using HttpResponseMessage response = await _fixture.Client!.SendAsync(request);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        using HttpRequestMessage finalRequest = new HttpRequestMessage(HttpMethod.Get, "/tags");
        finalRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        using HttpResponseMessage finalResponse = await _fixture.Client!.SendAsync(finalRequest);
        
        Assert.Equal(HttpStatusCode.TooManyRequests, finalResponse.StatusCode);
        Assert.NotNull(finalResponse.Headers.RetryAfter);
        Assert.True(finalResponse.Headers.RetryAfter.Delta.HasValue);
        Assert.InRange(finalResponse.Headers.RetryAfter.Delta.Value.TotalSeconds, 0, 60);
    }
}