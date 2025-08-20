# Simple API test
Write-Host "Testing API endpoints..."

# Test health endpoint
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5206/health" -Method GET
    Write-Host "Health endpoint works: $($health | ConvertTo-Json)" -ForegroundColor Green
}
catch {
    Write-Host "Health endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test users endpoint
try {
    $users = Invoke-RestMethod -Uri "http://localhost:5206/api/users" -Method GET
    Write-Host "Users endpoint works: $($users | ConvertTo-Json)" -ForegroundColor Green
}
catch {
    Write-Host "Users endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test registration endpoint
try {
    $body = @{
        email = "test@example.com"
        password = "SecurePassword123!"
        firstName = "John"
        lastName = "Doe"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "http://localhost:5206/api/users/register" -Method POST -ContentType "application/json" -Body $body
    Write-Host "Registration successful: $($response | ConvertTo-Json)" -ForegroundColor Green
}
catch {
    Write-Host "Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
} 