#!/usr/bin/env dotnet-script
// Usage: dotnet script seed-habits.csx
// This script seeds the database with 10 sample habits for testing

#r "nuget: Microsoft.EntityFrameworkCore, 10.0.7"
#r "nuget: Microsoft.EntityFrameworkCore.Design, 10.0.7"
#r "nuget: Npgsql.EntityFrameworkCore.PostgreSQL, 10.0.7"
#r "nuget: Microsoft.Extensions.Configuration, 8.0.0"
#r "nuget: Microsoft.Extensions.Configuration.Json, 8.0.0"

#load "src/HabitTracker.Api/Entities/Habit.cs"
#load "src/HabitTracker.Api/Data/ApplicationDbContext.cs"
#load "src/HabitTracker.Api/Data/Schemas.cs"

using Microsoft.EntityFrameworkCore;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Data;

// Sample user ID (placeholder until User entity is implemented)
var sampleUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");

// Create sample habits
var habits = new[]
{
    Habit.Create(
        sampleUserId,
        "Morning Exercise",
        "30 minutes of cardio or strength training",
        HabitType.Binary,
        new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
        null,
        null,
        new TimeOnly(6, 30)
    ),

    Habit.Create(
        sampleUserId,
        "Read",
        "Read at least 30 pages of a book",
        HabitType.Measurable,
        new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
        new Target { Value = 30, Unit = "pages" },
        null,
        new TimeOnly(20, 0)
    ),

    Habit.Create(
        sampleUserId,
        "Meditation",
        "Daily meditation practice",
        HabitType.Binary,
        new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
        null,
        null,
        new TimeOnly(7, 0)
    ),

    Habit.Create(
        sampleUserId,
        "Drink Water",
        "Drink 8 glasses of water throughout the day",
        HabitType.Measurable,
        new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 8 },
        new Target { Value = 8, Unit = "glasses" },
        null,
        null
    ),

    Habit.Create(
        sampleUserId,
        "Code Review",
        "Review at least one colleague's code",
        HabitType.Binary,
        new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 3 },
        null,
        null,
        new TimeOnly(14, 0)
    ),

    Habit.Create(
        sampleUserId,
        "Learn Something New",
        "Watch or read about a new technology or skill",
        HabitType.Binary,
        new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 2 },
        null,
        null,
        null
    ),

    Habit.Create(
        sampleUserId,
        "Run",
        "Run or jog for fitness",
        HabitType.Measurable,
        new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 3 },
        new Target { Value = 5, Unit = "km" },
        null,
        new TimeOnly(6, 0)
    ),

    Habit.Create(
        sampleUserId,
        "Journaling",
        "Reflect on the day in writing",
        HabitType.Binary,
        new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
        null,
        null,
        new TimeOnly(21, 0)
    ),

    Habit.Create(
        sampleUserId,
        "Healthy Meal",
        "Eat at least one fully prepared healthy meal",
        HabitType.Binary,
        new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
        null,
        null,
        null
    ),

    Habit.Create(
        sampleUserId,
        "Side Project",
        "Work on personal development project",
        HabitType.Measurable,
        new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 2 },
        new Target { Value = 60, Unit = "minutes" },
        null,
        new TimeOnly(19, 0)
    )
};

Console.WriteLine($"🌱 Seeding {habits.Length} sample habits...");

try
{
    // Create DbContext with connection string from environment or appsettings
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
        ?? "Host=localhost;Port=5432;Database=habittracker;Username=postgres;Password=postgres";
    
    optionsBuilder.UseNpgsql(connectionString);
    
    using var context = new ApplicationDbContext(optionsBuilder.Options);
    
    // Check if habits already exist
    var existingCount = context.Habits.Count(h => h.UserId == sampleUserId);
    if (existingCount > 0)
    {
        Console.WriteLine($"⚠️  Found {existingCount} existing habits for this user. Skipping seed to avoid duplicates.");
        Console.WriteLine("💡 To clear and reseed, run: dotnet ef database drop --project src/HabitTracker.Api/HabitTracker.Api.csproj");
        return;
    }
    
    // Add all habits
    context.Habits.AddRange(habits);
    var saveResult = context.SaveChanges();
    
    Console.WriteLine($"✅ Successfully added {saveResult} habits!");
    Console.WriteLine($"\n📋 Sample Habits Created:");
    Console.WriteLine($"   User ID: {sampleUserId}");
    
    foreach (var habit in habits)
    {
        Console.WriteLine($"   • {habit.Name} ({habit.Type}) - Frequency: {habit.Frequency.Type} x{habit.Frequency.TimesPerPeriod}");
    }
    
    Console.WriteLine($"\n💡 You can now test the API with these habits!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error seeding habits: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner error: {ex.InnerException.Message}");
    }
    Environment.Exit(1);
}
