using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.FeatureManagement;
using WebApiPrototype;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
