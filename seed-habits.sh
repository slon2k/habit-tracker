#!/bin/bash
# Seed the HabitTracker database with sample habits
# Usage: ./seed-habits.sh

echo ""
echo "🌱 HabitTracker Database Seeder"
echo "================================"
echo ""

# Build the seeder project
echo "Building seeder project..."
dotnet build src/HabitTracker.Seeder/HabitTracker.Seeder.csproj
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✅ Build succeeded"
echo ""

# Run the seeder
echo "Running seeder..."
dotnet run --project src/HabitTracker.Seeder/HabitTracker.Seeder.csproj

if [ $? -ne 0 ]; then
    echo "❌ Seeder failed"
    exit 1
fi

echo ""
echo "✅ Database seeding completed!"
echo ""
echo "💡 Next steps:"
echo "   - Run the API: dotnet run --project src/HabitTracker.Api/HabitTracker.Api.csproj"
echo "   - Visit: https://localhost:7016/swagger"
echo ""
