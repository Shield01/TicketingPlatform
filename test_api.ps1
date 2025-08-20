# Test script for User Registration API
Write-Host "Testing User Registration API..."

try {
    $body = @{
        email = "test@example.com"
        password = "SecurePassword123!"
        firstName = "John"
        lastName = "Doe"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:5206/api/users/register" -Method POST -ContentType "application/json" -Body $body
    
    Write-Host "Success! Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
} 