namespace WebApiPrototype
{
    public class MyCustomMiddleware : IMiddleware
    {
        private readonly ILogger<MyCustomMiddleware> _logger;
        public MyCustomMiddleware(ILogger<MyCustomMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogInformation($"MyCustomMiddleware - Before request {context.Request.Path}");
            await next(context);
            _logger.LogInformation($"MyCustomMiddleware - After request {context.Request.Path}");
        }
    }
}
