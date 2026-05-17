namespace HabitTracker.Api.Startup;

using HabitTracker.Api.Data;
using HabitTracker.Api.Services.Auth;
using Microsoft.AspNetCore.Identity;

public static class IdentityStartup
{
    public static WebApplicationBuilder AddIdentityServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register Identity for user/role management
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        // Register Token Service
        builder.Services.AddScoped<ITokenService, JwtTokenService>();

        return builder;
    }
}
