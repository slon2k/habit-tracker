using HabitTracker.Api.Data;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HabitTracker.Api.Extensions;

public static class DatabaseExtensions
{
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgOptions => npgOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

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

    private static readonly Action<ILogger, string, Exception?> LogAdminSeedFailed =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1002, nameof(ApplyMigrationsAsync)),
            "Failed to seed admin user: {Reason}");

    private static readonly Action<ILogger, Exception?> LogAdminSeedSkipped =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1003, nameof(ApplyMigrationsAsync)),
            "Admin seeding is disabled or missing required settings. Skipping.");

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
            await SeedAdminUserAsync(scope.ServiceProvider, dbContext, app.Logger);
        }
        catch (Exception ex)
        {
            LogMigrationFailed(app.Logger, ex);
            throw new InvalidOperationException("Failed to apply database migrations during application startup.", ex);
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider, ApplicationDbContext appDbContext, ILogger logger)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var options = configuration.GetSection(AdminSeedOptions.SectionName).Get<AdminSeedOptions>();

        if (options is null ||
            !options.Enabled ||
            string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.Password) ||
            string.IsNullOrWhiteSpace(options.Name))
        {
            LogAdminSeedSkipped(logger, null);
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(nameof(AppRole.Admin)))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole(nameof(AppRole.Admin)));
            if (!createRoleResult.Succeeded)
            {
                var reason = string.Join(", ", createRoleResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                LogAdminSeedFailed(logger, reason, null);
                throw new InvalidOperationException($"Failed to seed admin role. {reason}");
            }
        }

        var email = options.Email.Trim();
        var identityUser = await userManager.FindByEmailAsync(email);

        if (identityUser is null)
        {
            identityUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
            };

            var createUserResult = await userManager.CreateAsync(identityUser, options.Password);
            if (!createUserResult.Succeeded)
            {
                var reason = string.Join(", ", createUserResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                LogAdminSeedFailed(logger, reason, null);
                throw new InvalidOperationException($"Failed to seed admin identity user. {reason}");
            }
        }

        if (!await userManager.IsInRoleAsync(identityUser, nameof(AppRole.Admin)))
        {
            var addRoleResult = await userManager.AddToRoleAsync(identityUser, nameof(AppRole.Admin));
            if (!addRoleResult.Succeeded)
            {
                var reason = string.Join(", ", addRoleResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                LogAdminSeedFailed(logger, reason, null);
                throw new InvalidOperationException($"Failed to assign admin role to seeded user. {reason}");
            }
        }

        var appUser = await appDbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityUser.Id);
        if (appUser is null)
        {
            appDbContext.Users.Add(new User(Guid.NewGuid(), identityUser.Id, options.Name, email));
            await appDbContext.SaveChangesAsync();
        }
    }
}