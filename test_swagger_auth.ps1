# Test script for Swagger UI Authentication
Write-Host "Testing Ticketing Platform API Authentication..." -ForegroundColor Green

# Test 1: Health endpoint (should work without authentication)
Write-Host "`n1. Testing health endpoint (no auth required):" -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5206/health" -Method Get
    Write-Host "✅ Health endpoint working: $($healthResponse | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Health endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: User registration (should work without authentication)
Write-Host "`n2. Testing user registration (no auth required):" -ForegroundColor Yellow
$registerData = @{
    Email = "test@example.com"
    Password = "TestPassword123"
    FirstName = "Test"
    LastName = "User"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5206/api/users/register" -Method Post -Body $registerData -ContentType "application/json"
    Write-Host "✅ User registration working: $($registerResponse | ConvertTo-Json)" -ForegroundColor Green
    
    # Store the token for authenticated tests
    $token = $registerResponse.Token
    Write-Host "📝 JWT Token obtained: $($token.Substring(0, 50))..." -ForegroundColor Cyan
} catch {
    Write-Host "❌ User registration failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get user profile (should require authentication)
Write-Host "`n3. Testing get user profile (auth required):" -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $profileResponse = Invoke-RestMethod -Uri "http://localhost:5206/api/users/me" -Method Get -Headers $headers
    Write-Host "✅ Get user profile working with auth: $($profileResponse | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Get user profile failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get user profile without authentication (should fail)
Write-Host "`n4. Testing get user profile without auth (should fail):" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "http://localhost:5206/api/users/me" -Method Get
    Write-Host "❌ Get user profile should have failed without auth but succeeded" -ForegroundColor Red
} catch {
    Write-Host "✅ Get user profile correctly failed without auth: $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host "`n🎉 Swagger UI Authentication Test Complete!" -ForegroundColor Green
Write-Host "You can now:" -ForegroundColor Cyan
Write-Host "1. Open http://localhost:5206/swagger in your browser" -ForegroundColor White
Write-Host "2. Click the 'Authorize' button (lock icon)" -ForegroundColor White
Write-Host "3. Enter your JWT token in the format: Bearer {your-token}" -ForegroundColor White
Write-Host "4. Test authenticated endpoints directly from Swagger UI" -ForegroundColor White 