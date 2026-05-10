namespace HabitTracker.Api.Extensions;

using HabitTracker.Api.Data;
using HabitTracker.Api.Options;
using HabitTracker.Api.Services.Auth;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

public static class IdentityExtensions
{
    public static WebApplicationBuilder AddIdentityServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Key), "JWT key is required.")
            .Validate(static options => options.Key.Length >= 32, "JWT key must be at least 32 characters.")
            .Validate(static options => options.AccessTokenMinutes > 0, "JWT access token lifetime must be greater than zero.");

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        builder.Services.AddScoped<ITokenService, JwtTokenService>();

        return builder;
    }
}
