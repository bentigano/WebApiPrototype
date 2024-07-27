using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
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
