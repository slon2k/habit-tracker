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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Habit Tracker API v1");
        options.RoutePrefix = "swagger";
    });

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

public partial class Program { protected Program() { } }
