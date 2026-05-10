# Reset local development database, re-apply migrations, and seed sample data.
# Usage:
#   .\reset-dev-db.ps1
#   .\reset-dev-db.ps1 -SkipSeed

param(
    [switch]$SkipSeed
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProject = Join-Path $projectRoot "src\HabitTracker.Api\HabitTracker.Api.csproj"
$seederProject = Join-Path $projectRoot "src\HabitTracker.Seeder\HabitTracker.Seeder.csproj"

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Name" -ForegroundColor Cyan
    & $Action

    if ($LASTEXITCODE -ne 0) {
        throw "Step failed: $Name"
    }
}

Push-Location $projectRoot
try {
    Invoke-Step -Name "Build solution" -Action {
        dotnet build HabitTracker.slnx -v q
    }

    Invoke-Step -Name "Drop development database" -Action {
        dotnet ef database drop --force --context ApplicationDbContext --project $apiProject --startup-project $apiProject
    }

    Invoke-Step -Name "Apply application migrations" -Action {
        dotnet ef database update --context ApplicationDbContext --project $apiProject --startup-project $apiProject
    }

    Invoke-Step -Name "Apply identity migrations" -Action {
        dotnet ef database update --context ApplicationIdentityDbContext --project $apiProject --startup-project $apiProject
    }

    if (-not $SkipSeed) {
        Invoke-Step -Name "Seed sample data" -Action {
            $env:ASPNETCORE_ENVIRONMENT = 'Development'
            dotnet run --project $seederProject
        }
    }
    else {
        Write-Host ""
        Write-Host "==> Skipping seed step (-SkipSeed provided)" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "Development database reset completed." -ForegroundColor Green
}
finally {
    Pop-Location
}
