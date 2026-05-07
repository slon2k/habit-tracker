using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HabitTracker.Api.Data;

// Load configuration
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var seederProjectDir = Path.Combine(AppContext.BaseDirectory);
var config = new ConfigurationBuilder()
    .SetBasePath(seederProjectDir)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' not found in configuration");

// Create DbContext
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString)
    .Options;

using var context = new ApplicationDbContext(options);

Console.WriteLine("🌱 HabitTracker Sample Data Seeder");
Console.WriteLine("==================================\n");

try
{
    // Check database connection
    Console.WriteLine("🔗 Connecting to database...");
    await context.Database.OpenConnectionAsync();
    await context.Database.CloseConnectionAsync();
    Console.WriteLine("✅ Database connection successful\n");

    // Seed sample habits
    await SeedData.SeedSampleHabitsAsync(context);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Details: {ex.InnerException.Message}");
    }
    Environment.Exit(1);
}
