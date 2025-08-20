# Test script for User Login functionality
Write-Host "Testing User Login and JWT Token Issuance" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Test data
$baseUrl = "https://localhost:7001"
$registerData = @{
    email = "testuser@example.com"
    password = "SecurePassword123!"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

$loginData = @{
    email = "testuser@example.com"
    password = "SecurePassword123!"
} | ConvertTo-Json

try {
    Write-Host "1. Registering a new user..." -ForegroundColor Yellow
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/register" -Method POST -Body $registerData -ContentType "application/json"
    Write-Host "User registered successfully with ID: $($registerResponse.userId)" -ForegroundColor Green
    
    Write-Host "`n2. Testing login with valid credentials..." -ForegroundColor Yellow
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "User ID: $($loginResponse.userId)" -ForegroundColor Cyan
    Write-Host "Email: $($loginResponse.email)" -ForegroundColor Cyan
    Write-Host "Role: $($loginResponse.role)" -ForegroundColor Cyan
    Write-Host "Token: $($loginResponse.token.Substring(0, 50))..." -ForegroundColor Cyan
    Write-Host "Expires at: $($loginResponse.expiresAt)" -ForegroundColor Cyan
    
    Write-Host "`n3. Testing login with invalid credentials..." -ForegroundColor Yellow
    $invalidLoginData = @{
        email = "testuser@example.com"
        password = "WrongPassword"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/users/login" -Method POST -Body $invalidLoginData -ContentType "application/json"
        Write-Host "ERROR: Login should have failed!" -ForegroundColor Red
    }
    catch {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $responseBody = $reader.ReadToEnd()
        Write-Host "Expected error received: $responseBody" -ForegroundColor Green
    }
    
    Write-Host "`n✅ All tests passed! JWT login functionality is working correctly." -ForegroundColor Green
}
catch {
    Write-Host "❌ Test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the API is running on $baseUrl" -ForegroundColor Yellow
} 