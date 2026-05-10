namespace HabitTracker.Api.Extensions;

using HabitTracker.Api.Data;
using Microsoft.AspNetCore.Identity;

public static class IdentityExtensions
{
    public static WebApplicationBuilder AddIdentityServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        return builder;
    }
}
