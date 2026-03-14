using PersonalKnowledgeHub.Exceptions;

namespace PersonalKnowledgeHub.Middleware
{
    public class MiddlewareException
    {
        private readonly RequestDelegate _next;

        public MiddlewareException(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidCredentialException ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(ex.Message);
            }
        }
    }
}
