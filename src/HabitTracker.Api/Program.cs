using HabitTracker.Api.Extensions;
using HabitTracker.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<JwtDebugMiddleware>();

builder
    .AddApi()
    .AddDatabase()
    .AddIdentityServices()
    .AddTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();
app.UseMiddleware<JwtDebugMiddleware>();
app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
