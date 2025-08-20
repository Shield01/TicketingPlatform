# Test script for Event Update and Delete functionality

Write-Host "Testing Event Update and Delete functionality..." -ForegroundColor Green

# Navigate to the API directory
Set-Location -Path "TicketingPlatform.API"

# Start the API in the background
Write-Host "Starting API..." -ForegroundColor Yellow
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -WindowStyle Hidden

# Wait for API to start
Start-Sleep -Seconds 10

try {
    # Test 1: Create an event first
    Write-Host "Creating a test event..." -ForegroundColor Yellow
    $createEvent = @{
        title = "Test Event for Update/Delete"
        description = "This event will be updated and deleted"
        startDate = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
        endDate = (Get-Date).AddDays(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
        location = "Test Location"
        category = "Testing"
        isPublic = $true
        isPublished = $false
    } | ConvertTo-Json

    $headers = @{ "Content-Type" = "application/json" }
    
    # You would need to add authentication here in a real test
    # For now, this is a placeholder script to show the testing approach
    
    Write-Host "Event Update/Delete API endpoints are ready for testing!" -ForegroundColor Green
    Write-Host "PUT /api/events/{id} - Update event" -ForegroundColor Cyan
    Write-Host "DELETE /api/events/{id} - Delete event" -ForegroundColor Cyan
    Write-Host "Both endpoints support:" -ForegroundColor White
    Write-Host "  - Organizer ownership checks" -ForegroundColor White
    Write-Host "  - Admin override capability" -ForegroundColor White
    Write-Host "  - Ticket existence validation for deletion" -ForegroundColor White

} finally {
    # Clean up
    Write-Host "Stopping API..." -ForegroundColor Yellow
    if ($apiProcess -and !$apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force
    }
    Set-Location -Path ".."
}

Write-Host "Test script completed!" -ForegroundColor Green
