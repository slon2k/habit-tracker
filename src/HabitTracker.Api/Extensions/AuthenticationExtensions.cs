namespace HabitTracker.Api.Extensions;

using HabitTracker.Api.Options;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.Key), "JWT key must not be empty.")
            .Validate(static options => options.Key.Length >= 32, "JWT key must be at least 32 characters.")
            .Validate(static options => options.AccessTokenMinutes > 0, "JWT access token lifetime must be greater than zero.");

        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing from appsettings");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        builder.Services
            .AddAuthentication(options => options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtOptions.Issuer;
                options.Audience = jwtOptions.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}