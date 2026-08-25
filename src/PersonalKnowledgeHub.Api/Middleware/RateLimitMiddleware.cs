using System.Security.Claims;
using StackExchange.Redis;

namespace PersonalKnowledgeHub.Middleware
{
    public sealed class RateLimitMiddleware
    {
        private const int RequestLimit = 10;
        private const int WindowMilliseconds = 60_000;

        private const string IncrementScript = """
           local count = redis.call('INCR', KEYS[1])

           if count == 1 then 
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
           end

           local ttl = redis.call('PTTL', KEYS[1])
           return { count, ttl }
           """;

        private readonly IDatabase _redis;
        private readonly RequestDelegate _next;

        public RateLimitMiddleware(IConnectionMultiplexer connection, RequestDelegate next)
        {
            _redis = connection.GetDatabase();
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string key = "ratelimit:";

            string identity;

            if (context.User.Identity?.IsAuthenticated == true)
            {
                identity = "user:" + context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (context.Connection.RemoteIpAddress != null)
                {
                    identity = "ip:" + context.Connection.RemoteIpAddress;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid connection.");
                    return;
                }
            }

            key += identity;

            RedisResult result = await _redis.ScriptEvaluateAsync(
                IncrementScript,
                [ key ],
                [ WindowMilliseconds ]);
            
            RedisResult[] values = (RedisResult[])result!;
            long count = (long)values[0];
            long ttlMilliseconds = Math.Max(0, (long)values[1]);

            if (count > RequestLimit)
            {
                context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(ttlMilliseconds / 1000d)).ToString();
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Please try again later.");
                return;
            }
            
            await _next(context);
        }
    }
}
