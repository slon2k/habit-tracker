@echo off
REM Seed the HabitTracker database with sample habits
REM Usage: seed-habits.bat

echo.
echo 🌱 HabitTracker Database Seeder
echo ================================
echo.

REM Build the seeder project
echo Building seeder project...
dotnet build src\HabitTracker.Seeder\HabitTracker.Seeder.csproj
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed
    exit /b 1
)

echo ✅ Build succeeded
echo.

REM Run the seeder
echo Running seeder...
dotnet run --project src\HabitTracker.Seeder\HabitTracker.Seeder.csproj

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Seeder failed
    exit /b 1
)

echo.
echo ✅ Database seeding completed!
echo.
echo 💡 Next steps:
echo    - Run the API: dotnet run --project src/HabitTracker.Api/HabitTracker.Api.csproj
echo    - Visit: https://localhost:7016/swagger
echo.
pause
