@echo off
echo ========================================================
echo        Starting Agri Connect Microservices Ecosystem     
echo ========================================================
echo [1/6] Launching Admin Service on port 5001...
start "Admin Service (Port 5001)" cmd /k "cd /d ""%~dp0admin service"" && dotnet run --launch-profile http"

echo [2/6] Launching Farmer Service on port 5002...
start "Farmer Service (Port 5002)" cmd /k "cd /d ""%~dp0farmer service"" && dotnet run --launch-profile http"

echo [3/6] Launching Officer Service on port 5003...
start "Officer Service (Port 5003)" cmd /k "cd /d ""%~dp0officer service"" && dotnet run --launch-profile http"

echo [4/6] Launching Supplier Service on port 5004...
start "Supplier Service (Port 5004)" cmd /k "cd /d ""%~dp0supplier service"" && dotnet run --launch-profile http"

echo [5/6] Launching API Gateway on port 5000...
start "API Gateway (Port 5000)" cmd /k "cd /d ""%~dp0api-gateway"" && npm install && npm run dev"

echo [6/6] Launching Agri UI (Next.js) on port 3000...
start "Agri UI (Port 3000)" cmd /k "cd /d ""%~dp0agri-ui"" && npm install && npm run dev"

echo ========================================================
echo All services launched!
echo Agri UI:    http://localhost:3000
echo Gateway:    http://localhost:5000
echo ========================================================
pause
