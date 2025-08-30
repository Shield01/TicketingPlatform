# PowerShell script to apply EF Core migrations for all modules
# Usage: .\scripts\apply-migrations.ps1 [-Environment <env>]

param(
    [string]$Environment = "Development",
    [string]$ConnectionString = $null
)

Write-Host "Applying EF Core migrations for all modules..." -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = $Environment

if ($ConnectionString) {
    $env:POSTGRES_CONNECTION = $ConnectionString
    Write-Host "Using provided connection string" -ForegroundColor Yellow
}

# Set the working directory to the solution root
$SolutionRoot = Split-Path -Parent $PSScriptRoot
Set-Location $SolutionRoot

# Define modules and their contexts
$Modules = @(
    @{
        Name = "UserService"
        Path = "Modules.UserService"
        Context = "UserDbContext"
    },
    @{
        Name = "TeamService"
        Path = "Modules.TeamService"
        Context = "TeamServiceDbContext"
    },
    @{
        Name = "EventService"
        Path = "Modules.EventService"
        Context = "EventServiceDbContext"
    },
    @{
        Name = "TicketService"
        Path = "Modules.TicketService"
        Context = "TicketServiceDbContext"
    },
    @{
        Name = "PaymentService"
        Path = "Modules.PaymentService"
        Context = "PaymentServiceDbContext"
    }
)

# Check if database is accessible
Write-Host "`nChecking database connectivity..." -ForegroundColor Cyan

# Apply migrations for each module
$SuccessCount = 0
$TotalCount = $Modules.Count

foreach ($Module in $Modules) {
    Write-Host "`nApplying migrations for $($Module.Name)..." -ForegroundColor Cyan
    
    try {
        # Navigate to the module directory
        Set-Location $Module.Path
        
        # Apply the migration
        $Command = "dotnet ef database update --context $($Module.Context) --startup-project ../TicketingPlatform.API"
        Write-Host "Executing: $Command" -ForegroundColor DarkGray
        
        Invoke-Expression $Command
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[SUCCESS] Migrations applied successfully for $($Module.Name)" -ForegroundColor Green
            $SuccessCount++
        } else {
            Write-Host "[ERROR] Failed to apply migrations for $($Module.Name)" -ForegroundColor Red
        }
        
        # Return to solution root
        Set-Location $SolutionRoot
    }
    catch {
        Write-Host "[ERROR] Error applying migrations for $($Module.Name): $($_.Exception.Message)" -ForegroundColor Red
        Set-Location $SolutionRoot
    }
}

Write-Host "`n Migration Results:" -ForegroundColor Green
Write-Host "Successfully applied: $SuccessCount/$TotalCount modules" -ForegroundColor $(if ($SuccessCount -eq $TotalCount) { "Green" } else { "Yellow" })

if ($SuccessCount -eq $TotalCount) {
    Write-Host "SUCCESS: All migrations applied successfully!" -ForegroundColor Green
    Write-Host "`nYou can now start the application with:" -ForegroundColor Yellow
    Write-Host "dotnet run --project TicketingPlatform.API" -ForegroundColor White
} else {
    Write-Host "WARNING: Some migrations failed. Please check the errors above." -ForegroundColor Yellow
    Write-Host "Make sure your database is running and connection string is correct." -ForegroundColor White
}
