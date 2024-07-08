using WebApiPrototype;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();
