using Microsoft.Extensions.Primitives;

namespace WebApiPrototype.Middleware
{
    /// <summary>
    /// Middleware that will remove headers that provide system info,
    /// and add some additional headers per OWASP. See 
    /// https://owasp.org/www-project-secure-headers/
    /// </summary>
    public class MyHeaderSecurityMiddleware
    {
        private readonly RequestDelegate _next;

        public MyHeaderSecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                // remove some headers that OWASP recommends we remove
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");
                context.Response.Headers.Remove("X-AspNet-Version");
                context.Response.Headers.Remove("X-AspNetMvc-Version");
                context.Response.Headers.Append("X-Content-Type-Options", new StringValues("nosniff"));
                context.Response.Headers.Append("X-Frame-Options", new StringValues("DENY"));
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
