# Test user registration
Write-Host "Testing user registration..."

$body = @{
    email = "test@example.com"
    password = "SecurePassword123!"
    firstName = "John"
    lastName = "Doe"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5206/api/user/register" -Method POST -ContentType "application/json" -Body $body
    Write-Host "Registration successful!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Green
}
catch {
    Write-Host "Registration failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
} 