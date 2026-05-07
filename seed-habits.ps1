# Seed Database with Sample Habits
# Usage: .\seed-habits.ps1

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProject = Join-Path $projectRoot "src\HabitTracker.Api"

Write-Host "🌱 Preparing to seed sample habits..." -ForegroundColor Cyan

# Build the project
Write-Host "📦 Building project..." -ForegroundColor Yellow
Push-Location $projectRoot
try {
    dotnet build | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Build succeeded" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Create a temporary C# script to seed the database
$tempScript = Join-Path $env:TEMP "seed-habits-temp.csx"

$scriptContent = @"
#r `"nuget: Microsoft.EntityFrameworkCore, 10.0.7`"
#r `"nuget: Microsoft.EntityFrameworkCore.Design, 10.0.7`"
#r `"nuget: Npgsql.EntityFrameworkCore.PostgreSQL, 10.0.7`"

using Microsoft.EntityFrameworkCore;
using System.Reflection;

// Load the API assembly
var assembly = Assembly.LoadFrom(@"$apiProject\bin\Debug\net10.0\HabitTracker.Api.dll");

// Get the ApplicationDbContext type
var dbContextType = assembly.GetType("HabitTracker.Api.Data.ApplicationDbContext") 
    ?? throw new InvalidOperationException("Cannot find ApplicationDbContext type");

var seedDataType = assembly.GetType("HabitTracker.Api.Data.SeedData")
    ?? throw new InvalidOperationException("Cannot find SeedData type");

// Get method to seed
var seedMethod = seedDataType.GetMethod("SeedSampleHabitsAsync", 
    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
    ?? throw new InvalidOperationException("Cannot find SeedSampleHabitsAsync method");

// Create DbContext
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? "Host=localhost;Port=5432;Database=habittracker;Username=postgres;Password=postgres";

var optionsBuilder = new DbContextOptionsBuilder();
var useNpgsqlMethod = optionsBuilder.GetType().GetMethod("UseNpgsql",
    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

useNpgsqlMethod?.Invoke(optionsBuilder, new object[] { connectionString });

dynamic context = Activator.CreateInstance(dbContextType, optionsBuilder.Options) 
    ?? throw new InvalidOperationException("Cannot create DbContext instance");

try {
    var task = (Task)seedMethod.Invoke(null, new object[] { context })!;
    task.Wait();
    Console.WriteLine("\n✅ Seeding completed successfully!");
}
catch (Exception ex) {
    Console.WriteLine(`$"\n❌ Seeding failed: {ex.InnerException?.Message ?? ex.Message}`");
    throw;
}
finally {
    context?.Dispose();
}
"@

Set-Content -Path $tempScript -Value $scriptContent -Force

Write-Host "🔧 Running seed script..." -ForegroundColor Yellow

try {
    dotnet script $tempScript
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Sample habits seeded successfully!" -ForegroundColor Green
        Write-Host "📋 You can now test the API with the sample data." -ForegroundColor Cyan
    }
    else {
        Write-Host "`n❌ Seeding script failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
finally {
    Remove-Item -Path $tempScript -Force -ErrorAction SilentlyContinue
}
