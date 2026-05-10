using HabitTracker.Api.Extensions;
using HabitTracker.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
app.UseExceptionHandling();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
