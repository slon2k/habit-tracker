using HabitTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Data;

/// <summary>
/// Utility class for seeding the database with sample data.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Sample user identity used for development seeding.
    /// </summary>
    public static readonly Guid SampleUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");

    public const string SampleIdentityId = "dev-seed-identity-user-0001";

    public const string SampleUserName = "Sample User";

    public const string SampleUserEmail = "sample.user@habittracker.local";

    /// <summary>
    /// Seeds the database with 10 sample habits if none exist for the sample user.
    /// </summary>
    public static async Task SeedSampleHabitsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await EnsureSampleUserAsync(context, cancellationToken);

        // Check if habits already exist for the sample user
        var existingCount = await context.Habits.CountAsync(h => h.UserId == SampleUserId, cancellationToken);
        if (existingCount > 0)
        {
            Console.WriteLine($"ℹ️  Found {existingCount} existing habits for sample user. Skipping seed.");
            return;
        }

        var habits = new List<Habit>
        {
            Habit.Create(
                SampleUserId,
                "Morning Exercise",
                "30 minutes of cardio or strength training",
                HabitType.Binary,
                new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
                null,
                null,
                new TimeOnly(6, 30)
            ),

            Habit.Create(
                SampleUserId,
                "Read",
                "Read at least 30 pages of a book",
                HabitType.Measurable,
                new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
                new Target { Value = 30, Unit = "pages" },
                null,
                new TimeOnly(20, 0)
            ),

            Habit.Create(
                SampleUserId,
                "Meditation",
                "Daily meditation practice",
                HabitType.Binary,
                new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
                null,
                null,
                new TimeOnly(7, 0)
            ),

            Habit.Create(
                SampleUserId,
                "Drink Water",
                "Drink 8 glasses of water throughout the day",
                HabitType.Measurable,
                new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 8 },
                new Target { Value = 8, Unit = "glasses" },
                null,
                null
            ),

            Habit.Create(
                SampleUserId,
                "Code Review",
                "Review at least one colleague's code",
                HabitType.Binary,
                new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 3 },
                null,
                null,
                new TimeOnly(14, 0)
            ),

            Habit.Create(
                SampleUserId,
                "Learn Something New",
                "Watch or read about a new technology or skill",
                HabitType.Binary,
                new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 2 },
                null,
                null,
                null
            ),

            Habit.Create(
                SampleUserId,
                "Run",
                "Run or jog for fitness",
                HabitType.Measurable,
                new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 3 },
                new Target { Value = 5, Unit = "km" },
                null,
                new TimeOnly(6, 0)
            ),

            Habit.Create(
                SampleUserId,
                "Journaling",
                "Reflect on the day in writing",
                HabitType.Binary,
                new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
                null,
                null,
                new TimeOnly(21, 0)
            ),

            Habit.Create(
                SampleUserId,
                "Healthy Meal",
                "Eat at least one fully prepared healthy meal",
                HabitType.Binary,
                new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
                null,
                null,
                null
            ),

            Habit.Create(
                SampleUserId,
                "Side Project",
                "Work on personal development project",
                HabitType.Measurable,
                new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 2 },
                new Target { Value = 60, Unit = "minutes" },
                null,
                new TimeOnly(19, 0)
            )
        };

        Console.WriteLine($"🌱 Seeding {habits.Count} sample habits...");
        context.Habits.AddRange(habits);

        try
        {
            var saved = await context.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"✅ Successfully added {saved} habits!");
            Console.WriteLine($"\n📋 Sample Habits Created:");
            foreach (var habit in habits)
            {
                Console.WriteLine($"   • {habit.Name} ({habit.Type}) - Frequency: {habit.Frequency.Type} x{habit.Frequency.TimesPerPeriod}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error seeding habits: {ex.Message}");
            throw;
        }
    }

    private static async Task EnsureSampleUserAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.AnyAsync(u => u.Id == SampleUserId, cancellationToken);
        if (existingUser)
        {
            return;
        }

        Console.WriteLine("👤 Creating sample user for development seeding...");

        context.Users.Add(new User(
            SampleUserId,
            SampleIdentityId,
            SampleUserName,
            SampleUserEmail));

        await context.SaveChangesAsync(cancellationToken);
    }
}
