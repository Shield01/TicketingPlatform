# PowerShell script to reset the database and apply fresh migrations
# Usage: .\scripts\reset-database.ps1 [-Environment <env>] [-Confirm]

param(
    [string]$Environment = "Development",
    [switch]$Confirm = $false
)

if (-not $Confirm) {
    Write-Host "⚠️  WARNING: This will delete all data in the database!" -ForegroundColor Red
    Write-Host "This action cannot be undone." -ForegroundColor Red
    $response = Read-Host "Are you sure you want to continue? (type 'yes' to confirm)"
    
    if ($response -ne "yes") {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Resetting database for environment: $Environment" -ForegroundColor Green

# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = $Environment

# Set the working directory to the solution root
$SolutionRoot = Split-Path -Parent $PSScriptRoot
Set-Location $SolutionRoot

# Define modules and their contexts (in reverse order for dropping)
$Modules = @(
    @{
        Name = "PaymentService"
        Path = "Modules.PaymentService"
        Context = "PaymentServiceDbContext"
    },
    @{
        Name = "TicketService"
        Path = "Modules.TicketService"
        Context = "TicketServiceDbContext"
    },
    @{
        Name = "EventService"
        Path = "Modules.EventService"
        Context = "EventServiceDbContext"
    },
    @{
        Name = "TeamService"
        Path = "Modules.TeamService"
        Context = "TeamServiceDbContext"
    },
    @{
        Name = "UserService"
        Path = "Modules.UserService"
        Context = "UserDbContext"
    }
)

# Drop databases for each module
Write-Host "`nDropping existing databases..." -ForegroundColor Cyan

foreach ($Module in $Modules) {
    Write-Host "Dropping database for $($Module.Name)..." -ForegroundColor DarkCyan
    
    try {
        Set-Location $Module.Path
        
        $Command = "dotnet ef database drop --context $($Module.Context) --startup-project ../TicketingPlatform.API --force"
        Write-Host "Executing: $Command" -ForegroundColor DarkGray
        
        Invoke-Expression $Command
        
        Set-Location $SolutionRoot
    }
    catch {
        Write-Host "Note: Could not drop database for $($Module.Name) (may not exist)" -ForegroundColor DarkGray
        Set-Location $SolutionRoot
    }
}

# Now apply fresh migrations
Write-Host "`nApplying fresh migrations..." -ForegroundColor Green

# Reverse the order for creating
[array]::Reverse($Modules)

foreach ($Module in $Modules) {
    Write-Host "Creating database for $($Module.Name)..." -ForegroundColor Cyan
    
    try {
        Set-Location $Module.Path
        
        $Command = "dotnet ef database update --context $($Module.Context) --startup-project ../TicketingPlatform.API"
        Write-Host "Executing: $Command" -ForegroundColor DarkGray
        
        Invoke-Expression $Command
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Database created successfully for $($Module.Name)" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to create database for $($Module.Name)" -ForegroundColor Red
        }
        
        Set-Location $SolutionRoot
    }
    catch {
        Write-Host "❌ Error creating database for $($Module.Name): $($_.Exception.Message)" -ForegroundColor Red
        Set-Location $SolutionRoot
    }
}

Write-Host "`n🎉 Database reset completed!" -ForegroundColor Green
Write-Host "The database has been reset with fresh schema." -ForegroundColor White
