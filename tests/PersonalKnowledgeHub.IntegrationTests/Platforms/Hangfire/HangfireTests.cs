using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Hangfire;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Platforms.Hangfire;

[Collection(nameof(HangfireCollection))]
public class HangfireTests
{
    private readonly HangfireFixture _fixture;

    public HangfireTests(HangfireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CleanUpRefreshTokens_WhenCalled_CleanUpRefreshTokens()
    {
        await using var scope = _fixture.Factory!.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        User user = new User
        {
            Email = "user@gmail.com",
            PasswordHash = "user password",
            Status = UserStatus.Active
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        Guid familyId = Guid.NewGuid();
        string token1 = await tokenService.GenerateRefreshToken(user.Id, familyId, CancellationToken.None);
        string token2 = await tokenService.GenerateRefreshToken(user.Id, familyId, CancellationToken.None);
        string token3 = await tokenService.GenerateRefreshToken(user.Id, familyId, CancellationToken.None);
        
        RefreshToken refreshToken1 = await tokenService.GetRefreshToken(token1, CancellationToken.None);
        refreshToken1.Revoked = true;
        refreshToken1.ExpiresAt = DateTime.UtcNow.AddDays(-31);
        await  dbContext.SaveChangesAsync();
        
        RefreshToken refreshToken2 = await tokenService.GetRefreshToken(token2, CancellationToken.None);
        refreshToken2.Revoked = true;
        refreshToken2.ExpiresAt = DateTime.UtcNow.AddDays(-31);
        await dbContext.SaveChangesAsync();
        
        RefreshToken refreshToken3 = await tokenService.GetRefreshToken(token3, CancellationToken.None);
        
        backgroundJobClient.Enqueue<ITokenService>(service => service.CleanUpRefreshTokens(CancellationToken.None));

        var timeout = DateTime.UtcNow.AddSeconds(10);
        List<RefreshToken> remainingTokens = [];
        
        do
        {
            await Task.Delay(100);
            
            await using var assertionScope = _fixture.Factory!.Services.CreateAsyncScope();
            var assertDbContext =  assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();

            remainingTokens =
                await assertDbContext.RefreshTokens.AsNoTracking().Where(rf => rf.UserId == user.Id).ToListAsync();
        }
        while (remainingTokens.Count != 1 && DateTime.UtcNow < timeout);
        
        Assert.Single(remainingTokens);
        Assert.Equal(refreshToken3.Id, remainingTokens[0].Id);
    }
}