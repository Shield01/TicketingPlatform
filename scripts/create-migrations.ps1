# PowerShell script to create EF Core migrations for all modules
# Usage: .\scripts\create-migrations.ps1 [MigrationName]

param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName = "InitialCreate"
)

Write-Host "Creating EF Core migrations for all modules..." -ForegroundColor Green
Write-Host "Migration Name: $MigrationName" -ForegroundColor Yellow

# Set the working directory to the solution root
$SolutionRoot = Split-Path -Parent $PSScriptRoot
Set-Location $SolutionRoot

# Define modules and their contexts
$Modules = @(
    @{
        Name = "UserService"
        Path = "Modules.UserService"
        Context = "UserDbContext"
        Assembly = "Modules.UserService"
    },
    @{
        Name = "EventService"
        Path = "Modules.EventService"
        Context = "EventServiceDbContext"
        Assembly = "Modules.EventService"
    },
    @{
        Name = "TeamService" 
        Path = "Modules.TeamService"
        Context = "TeamServiceDbContext"
        Assembly = "Modules.TeamService"
    },
    @{
        Name = "TicketService"
        Path = "Modules.TicketService"
        Context = "TicketServiceDbContext"
        Assembly = "Modules.TicketService"
    },
    @{
        Name = "PaymentService"
        Path = "Modules.PaymentService"
        Context = "PaymentServiceDbContext"
        Assembly = "Modules.PaymentService"
    }
)

# Create migrations for each module
foreach ($Module in $Modules) {
    Write-Host "`nProcessing $($Module.Name)..." -ForegroundColor Cyan
    
    try {
        # Navigate to the module directory
        Set-Location $Module.Path
        
        # Create the migration
        $Command = "dotnet ef migrations add $MigrationName --context $($Module.Context) --output-dir Data/Migrations --startup-project ../TicketingPlatform.API"
        Write-Host "Executing: $Command" -ForegroundColor DarkGray
        
        Invoke-Expression $Command
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migration created successfully for $($Module.Name)" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to create migration for $($Module.Name)" -ForegroundColor Red
        }
        
        # Return to solution root
        Set-Location $SolutionRoot
    }
    catch {
        Write-Host "❌ Error creating migration for $($Module.Name): $($_.Exception.Message)" -ForegroundColor Red
        Set-Location $SolutionRoot
    }
}

Write-Host "`n🎉 Migration creation process completed!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Review the generated migrations in each module's Data/Migrations folder" -ForegroundColor White
Write-Host "2. Run .\scripts\apply-migrations.ps1 to apply migrations to the database" -ForegroundColor White
Write-Host "3. Or use 'docker-compose up -d' to start with a fresh database" -ForegroundColor White
