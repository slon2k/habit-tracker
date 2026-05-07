# Development Guide

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [dotnet-ef CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

## Running the Application

### Option A: Full Docker (all services)

```powershell
docker-compose up --build
```

- API: https://localhost:5001
- Aspire Dashboard: http://localhost:18888

### Option B: Local API + Dockerized Database (recommended for active development)

```powershell
# Start infrastructure only (Postgres + Aspire Dashboard)
docker-compose up aspire-dashboard postgres

# In another terminal, stop the API container if it's running
docker-compose stop habittracker-api

# Run the API locally
dotnet run --project src/HabitTracker.Api/HabitTracker.Api.csproj
```

The API picks up `appsettings.Development.json` automatically (`ASPNETCORE_ENVIRONMENT=Development`), which connects to the Dockerized Postgres on `localhost:5432`.

## Database

### Apply Migrations

```powershell
dotnet ef database update \
  --project src/HabitTracker.Api/HabitTracker.Api.csproj \
  --startup-project src/HabitTracker.Api/HabitTracker.Api.csproj
```

> In Docker mode, migrations are applied automatically on startup.

### Add a New Migration

```powershell
dotnet ef migrations add <MigrationName> \
  --project src/HabitTracker.Api/HabitTracker.Api.csproj \
  --startup-project src/HabitTracker.Api/HabitTracker.Api.csproj \
  --output-dir Data/Migrations
```

### Seed Sample Data

```powershell
# Uses appsettings.Development.json (localhost:5432, postgres:postgres)
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/HabitTracker.Seeder/HabitTracker.Seeder.csproj
```

Or use the convenience script:

```powershell
.\seed-habits.bat
```

Seeds 10 sample habits for `UserId = 550e8400-e29b-41d4-a716-446655440001`. Seeding is idempotent — re-running will skip if habits already exist.

## Docker Cheatsheet

| Goal | Command |
|------|---------|
| Full rebuild + restart all services | `docker-compose up --build` |
| Rebuild API only | `docker-compose up --build habittracker-api` |
| Restart API without rebuild | `docker-compose restart habittracker-api` |
| Stop API (keep DB running) | `docker-compose stop habittracker-api` |
| Full reset (deletes database volume) | `docker-compose down -v` |

## Connection Strings

| Context | Value |
|---------|-------|
| Local (Development) | `Host=localhost;Port=5432;Database=habittracker;Username=ht;Password=habittracker` |
| Docker internal | `Host=postgres;Port=5432;Database=habittracker;Username=ht;Password=habittracker` |

> To override locally, set the `CONNECTION_STRING` environment variable or edit `appsettings.Development.json`.
