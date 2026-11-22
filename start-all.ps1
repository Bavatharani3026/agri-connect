# Agri Connect - Start All Microservices
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "       Starting Agri Connect Microservices Ecosystem     " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

$root = $PSScriptRoot

Write-Host "[1/6] Launching Admin Service on port 5001..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root\admin service'; dotnet run --launch-profile http"

Write-Host "[2/6] Launching Farmer Service on port 5002..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root\farmer service'; dotnet run --launch-profile http"

Write-Host "[3/6] Launching Officer Service on port 5003..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root\officer service'; dotnet run --launch-profile http"

Write-Host "[4/6] Launching Supplier Service on port 5004..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root\supplier service'; dotnet run --launch-profile http"

Write-Host "[5/6] Launching API Gateway on port 5000..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root\api-gateway'; npm install; npm run dev"

Write-Host "[6/6] Launching Agri UI (Next.js) on port 3000..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root\agri-ui'; npm install; npm run dev"

Write-Host "========================================================" -ForegroundColor Green
Write-Host "All services initiated in separate windows!" -ForegroundColor Green
Write-Host "  Agri UI:     http://localhost:3000" -ForegroundColor Cyan
Write-Host "  API Gateway: http://localhost:5000" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Green
