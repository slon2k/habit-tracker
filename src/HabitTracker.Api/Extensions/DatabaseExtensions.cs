using HabitTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HabitTracker.Api.Extensions;

public static class DatabaseExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgOptions => npgOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application)));

        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgOptions => npgOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity)));

        return builder;
    }

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
        using ApplicationIdentityDbContext identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            await identityDbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            LogMigrationFailed(app.Logger, ex);
            throw new InvalidOperationException("Failed to apply database migrations during application startup.", ex);
        }
    }
}