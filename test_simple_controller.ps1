# Test simple controller
Write-Host "Testing simple controller..."

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5206/api/test" -Method GET
    Write-Host "Simple controller works: $($response | ConvertTo-Json)" -ForegroundColor Green
}
catch {
    Write-Host "Simple controller failed: $($_.Exception.Message)" -ForegroundColor Red
} 