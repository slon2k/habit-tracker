using HabitTracker.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Testcontainers.PostgreSql;

namespace HabitTracker.Api.IntegrationTests;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestIssuer = "https://habit-tracker-tests";
    private const string TestAudience = "habit-tracker-tests-clients";
    private const string TestSigningKey = "integration-tests-signing-key-123456";

    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("habittracker_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public IntegrationTestWebApplicationFactory()
    {
        postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
    }

    public override async ValueTask DisposeAsync()
    {
        await postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Issuer", TestIssuer);
        builder.UseSetting("Jwt:Audience", TestAudience);
        builder.UseSetting("Jwt:Key", TestSigningKey);
        builder.UseSetting("Jwt:AccessTokenMinutes", "30");
        builder.UseSetting("Jwt:RefreshTokenDays", "7");
        builder.UseSetting("ConnectionStrings:Database", postgreSqlContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationIdentityDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(
                        postgreSqlContainer.GetConnectionString(),
                        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddDbContext<ApplicationIdentityDbContext>(options =>
                options.UseNpgsql(
                    postgreSqlContainer.GetConnectionString(),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity)));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                    new OpenIdConnectConfiguration());
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey)),
                    ValidateIssuer = true,
                    ValidIssuer = TestIssuer,
                    ValidateAudience = true,
                    ValidAudience = TestAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                };
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>().Database.Migrate();
        });
    }
}
