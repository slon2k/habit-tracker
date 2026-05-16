namespace HabitTracker.Api.Extensions;

using HabitTracker.Api.Data;
using HabitTracker.Api.Options;
using HabitTracker.Api.Services.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class IdentityExtensions
{
    public static WebApplicationBuilder AddIdentityServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Validate JWT options early
        builder.Services
            .AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Key), "JWT key must not be empty.")
            .Validate(static options => options.Key.Length >= 32, "JWT key must be at least 32 characters.")
            .Validate(static options => options.AccessTokenMinutes > 0, "JWT access token lifetime must be greater than zero.");

        // Load JWT options from configuration
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing from appsettings");

        // Create the signing key using UTF8 encoding (MUST match token generation)
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        // Configure JWT Bearer Authentication - explicitly specify scheme
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {
                // Configure token validation parameters - MUST match token generation exactly
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Signing key
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    // Issuer/Audience
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    // Lifetime
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                };

                // CRITICAL: Disable HTTPS metadata requirement for HTTP localhost development
                options.RequireHttpsMetadata = false;
            });

        // Register Identity for user/role management
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        // Register Authorization service
        builder.Services.AddAuthorization();

        // Register Token Service
        builder.Services.AddScoped<ITokenService, JwtTokenService>();

        return builder;
    }
}
