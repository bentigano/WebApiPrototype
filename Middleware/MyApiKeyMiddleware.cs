namespace WebApiPrototype.Middleware
{
    /// <summary>
    /// Check to see if an X-Api-Key header was supplied and the valied is allowed based on a whitelist.
    /// If not, return a 401 and a response of "Invalid or missing API Key".
    /// </summary>
    public class MyApiKeyMiddleware
    {
        private const string API_KEY_HEADER_NAME = "X-Api-Key";
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public MyApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // retrieve the allowed API keys as a list from the appsettings.json file
            // (could also retrieve from a database or other source)
            var _validApiKeys = _configuration.GetSection("AllowedApiKeys").Get<string[]>();
            if (_validApiKeys == null) { _validApiKeys = new string[0]; }


            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var receivedApiKey) || !_validApiKeys.Contains(receivedApiKey.ToString()))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync($"Invalid or missing API Key.");
                return;
            }

            await _next(context);
        }
    }
}
