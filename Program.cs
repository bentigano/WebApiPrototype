using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using System.Text;
using WebApiPrototype;
using WebApiPrototype.HealthChecks;
using WebApiPrototype.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom Middleware
builder.Services.AddTransient<MyCustomMiddleware>();

#region Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});
#endregion

#region Rate Limiting
builder.Services.AddRateLimiter(limitOptions =>
{
    limitOptions.AddFixedWindowLimiter(policyName: "FiveRequestsPerTenSeconds", fixedWindowOptions =>
    {
        fixedWindowOptions.PermitLimit = 5;
        fixedWindowOptions.Window = TimeSpan.FromSeconds(10);
        fixedWindowOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        fixedWindowOptions.QueueLimit = 2;
    });
});
#endregion

// Feature Management (by config)
builder.Services.AddFeatureManagement();
builder.Services.AddFeatureManagement().AddFeatureFilter<PercentageFilter>();

// Custom Configuration Options
builder.Services.Configure<CustomOptions>(
    builder.Configuration.GetSection("CustomOptions"));

// Custom services
builder.Services.AddHostedService<MyBackgroundService>();
builder.Services.AddHostedService<MyPeriodicBackgroundService>();

// Health Checks
builder.Services.AddHealthChecks();
builder.Services.AddHealthChecks()
    .AddCheck<MyHealthCheck>("MySampleHealthCheckName");

// Caching
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// minimal API's
app.MapGet("/GetUser", () =>
{
    return "User retrieved";
});
app.MapDelete("/DeleteUser", (int userNumber) =>
{
    return $"User with number {userNumber} deleted";
});

#region Minimal API's with Parameters
{
    app.MapPut("/SaveUser/{id}", ([FromRoute] int id,
                         [FromQuery(Name = "name")] string fullName,
                         [FromHeader(Name = "Content-Type")] string contentType)
                         =>
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"id = {id}");
        sb.AppendLine($"name = {fullName}");
        sb.AppendLine($"contentType = {contentType}");
        return sb.ToString();
    });
    // Same request as above but pulling the parameters directly from the request
    // This approach will not show ANY parameters for this request in Swagger (unlike above)
    app.MapPut("/DeactivateUser/{id}", (HttpRequest request) =>
    {
        var id = request.RouteValues["id"];
        var fullName = request.Query["name"];
        var contentType = request.Headers["Content-Type"];
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"id = {id}");
        sb.AppendLine($"name = {fullName}");
        sb.AppendLine($"contentType = {contentType}");
        return sb.ToString();
    });
    // Properties from the body will be mapped to properties of WeatherForecast.
    // Any JSON values that aren't properties of WeatherForecast will be ignored.
    // See Swagger for example
    app.MapPatch("/CreateForecast/{id}", (WeatherForecast forecast) =>
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"summary = {forecast.Summary}");
        sb.AppendLine($"date = {forecast.Date}");
        sb.AppendLine($"TempC = {forecast.TemperatureC}");
        return sb.ToString();
    });
}
#endregion

// Extension method created in MyUserEndpoints.cs
app.AddMyUserEndpoints();

// must be called after routing, for example when using [EnableRateLimiting] on controllers and endpoints
app.UseRateLimiter();

app.UseMiddleware<MyCustomMiddleware>();

// IP Whitelist Middleware
app.UseMiddleware<MyIPWhitelistMiddleware>();

// API Key Middleware
app.UseMiddleware<MyApiKeyMiddleware>();

// Remove Insecure Headers
app.UseMiddleware<MyHeaderSecurityMiddleware>();

// map custom middleware with a request delegate (NOT the preferred way, which is above)
app.Use(async (context, next) =>
{
    System.Diagnostics.Debug.WriteLine($"Request Delegate Middleware - Before request {context.Request.Path}");
    await next(context);
    System.Diagnostics.Debug.WriteLine($"Request Delegate Middleware - After request {context.Request.Path}");
});

// map health check URL with default options
// app.MapHealthChecks("/healthchecks");
// map health check URL with specific HTTP response codes (below are the defaults)
app.MapHealthChecks("/healthchecks", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status505HttpVersionNotsupported
    },
    ResponseWriter = HealthCheckOutputWriter.WriteResponse
});

app.Run();
