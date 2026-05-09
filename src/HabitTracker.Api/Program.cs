using HabitTracker.Api.Extensions;
using HabitTracker.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApi();
builder.AddDatabase();
builder.AddTelemetry();

var app = builder.Build();

app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
