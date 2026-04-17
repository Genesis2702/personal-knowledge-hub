using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace PersonalKnowledgeHub.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RequestDelegate _next;
        private readonly int _requestPerMinute = 10;

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
                string ip;
                if (context.Connection.RemoteIpAddress == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Connection is invalid");
                    return;
                }
                ip = context.Connection.RemoteIpAddress.ToString();
                rateLimitKey = $"ratelimit:{ip}";
            }
            string? cachedCounter = await _distributedCache.GetStringAsync(rateLimitKey);
            if (cachedCounter == null)
            {
                DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                DateTime expiry = DateTime.UtcNow.AddSeconds(60);
                await _distributedCache.SetStringAsync(rateLimitKey, $"1|{expiry}", cacheEntryOption);
                await _next(context);
            }
            else
            {
                string[] splitCounter = cachedCounter.Split('|');
                int counter = int.Parse(splitCounter[0]);
                DateTime expiry = DateTime.Parse(splitCounter[1]);
                if (counter >= _requestPerMinute)
                {
                    context.Response.Headers.RetryAfter = (expiry - DateTime.UtcNow).ToString();
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Please try again later");
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
