using System.Net;

namespace WebApiPrototype
{
    /// <summary>
    /// Check to see if the IP address of the client is allowed based on a whitelist.
    /// If not, return a 403 and a response of "Access denied".
    /// </summary>
    public class MyIPWhitelistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public MyIPWhitelistMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (remoteIp == null) { remoteIp = IPAddress.None.ToString(); }
            // retrieve the allowed IP's as a list from the appsettings.json file
            // (could also retrieve from a database or other source)
            var allowedIPs = _configuration.GetSection("AllowedIPs").Get<string[]>();

            if (allowedIPs == null) { allowedIPs = new string[0]; }

            if (!IPAddress.IsLoopback(IPAddress.Parse(remoteIp)) && !allowedIPs.Contains(remoteIp))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access denied");
                return;
            }

            await _next(context);
        }
    }
}
