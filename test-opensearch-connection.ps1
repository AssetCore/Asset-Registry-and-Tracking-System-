# Test OpenSearch Connection Script
Write-Host "Testing OpenSearch Connection..." -ForegroundColor Cyan
Write-Host ""

$opensearchUri = "http://3.150.64.215:9200"
$username = "admin"
$password = "MyStrongPassword123!"

# Test 1: Basic connectivity
Write-Host "Test 1: Basic connectivity to OpenSearch..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $opensearchUri -Method GET -TimeoutSec 10
    Write-Host "✓ Connection successful! Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content.Substring(0, [Math]::Min(200, $response.Content.Length)))" -ForegroundColor Gray
} catch {
    Write-Host "✗ Connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "This means OpenSearch is not accessible from your machine." -ForegroundColor Yellow
    Write-Host "Check: Firewall, Security Group, or OpenSearch not running" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 2: Authentication
Write-Host "Test 2: Testing authentication..." -ForegroundColor Yellow
try {
    $pair = "$($username):$($password)"
    $encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
    $headers = @{ Authorization = "Basic $encodedCreds" }
    $response = Invoke-WebRequest -Uri $opensearchUri -Headers $headers -TimeoutSec 10
    Write-Host "✓ Authentication successful!" -ForegroundColor Green
} catch {
    Write-Host "✗ Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Check your username and password in appsettings.json" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 3: List indices
Write-Host "Test 3: Listing existing indices..." -ForegroundColor Yellow
try {
    $pair = "$($username):$($password)"
    $encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
    $headers = @{ Authorization = "Basic $encodedCreds" }
    $response = Invoke-WebRequest -Uri "$opensearchUri/_cat/indices?v" -Headers $headers -TimeoutSec 10
    Write-Host "✓ Successfully retrieved indices:" -ForegroundColor Green
    Write-Host $response.Content -ForegroundColor Gray
} catch {
    Write-Host "✗ Failed to list indices: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "If all tests pass, your connection is working!" -ForegroundColor Green
Write-Host "If indices don't appear, check:" -ForegroundColor Yellow
Write-Host "  1. Service console for errors" -ForegroundColor Yellow
Write-Host "  2. Local log files in logs/ directory" -ForegroundColor Yellow
Write-Host "  3. Wait 30-60 seconds after service starts" -ForegroundColor Yellow
