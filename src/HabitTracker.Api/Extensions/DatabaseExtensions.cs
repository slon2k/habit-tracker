using HabitTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Extensions;

public static class DatabaseExtensions
{
    private static readonly Action<ILogger, Exception?> LogMigrationFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1001, nameof(ApplyMigrationsAsync)),
            "An error occurred while applying database migrations.");

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        using IServiceScope scope = app.Services.CreateScope();
        using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogMigrationFailed(app.Logger, ex);
            throw new InvalidOperationException("Failed to apply database migrations during application startup.", ex);
        }
    }
}