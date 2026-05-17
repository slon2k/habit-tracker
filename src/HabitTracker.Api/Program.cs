using HabitTracker.Api.Startup;
using HabitTracker.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApi()
    .AddDatabase()
    .AddIdentityServices()
    .AddAuthenticationServices()
    .AddTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();
app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
