using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace PersonalKnowledgeHub.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RequestDelegate _next;
        private readonly int RequestPerMinute = 10;

        public RateLimitMiddleware(IDistributedCache distributedCache, RequestDelegate next)
        {
            _distributedCache = distributedCache;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string rateLimitKey;
            if (context.User.Identity!.IsAuthenticated)
            {
                string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                rateLimitKey = $"ratelimit:{userId}";
            }
            else
            {
                string ip = context.Connection.RemoteIpAddress != null ? context.Connection.RemoteIpAddress.ToString() : "unknown";
                rateLimitKey = $"ratelimit:{ip}";
            }
            string? cachedCounter = await _distributedCache.GetStringAsync(rateLimitKey);
            if (cachedCounter == null)
            {
                DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                await _distributedCache.SetStringAsync(rateLimitKey, "1", cacheEntryOption);
            }
            else
            {
                int counter = int.Parse(cachedCounter);
                if (counter >= RequestPerMinute)
                {
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Please try again later");
                    return;
                }
                else
                {
                    await _distributedCache.SetStringAsync(rateLimitKey, (counter + 1).ToString());
                    await _next(context);
                }
            }
        }

    }
}
