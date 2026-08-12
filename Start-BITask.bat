@echo off
setlocal
set "ROOT=%~dp0"

echo ============================================
echo   BITask - starting backend + frontend
echo ============================================
echo.

echo [1/4] Starting AuthService (http://localhost:5001)...
start "BITask - AuthService" cmd /k "cd /d "%ROOT%" && dotnet run --project src\Services\AuthService\AuthService.csproj --urls http://localhost:5001"
timeout /t 5 /nobreak >nul

echo [2/4] Starting ProductService (http://localhost:5002)...
start "BITask - ProductService" cmd /k "cd /d "%ROOT%" && dotnet run --project src\Services\ProductService\ProductService.csproj --urls http://localhost:5002"
timeout /t 5 /nobreak >nul

echo [3/4] Starting Gateway (http://localhost:5000)...
start "BITask - Gateway" cmd /k "cd /d "%ROOT%" && dotnet run --project src\Gateway\Gateway.csproj --urls http://localhost:5000"
timeout /t 5 /nobreak >nul

echo [4/4] Starting Angular frontend (http://localhost:4200)...
if not exist "%ROOT%frontend\node_modules" (
    echo       First run detected - installing frontend dependencies, this can take a minute...
    pushd "%ROOT%frontend"
    call npm install
    popd
)
start "BITask - Frontend" cmd /k "cd /d "%ROOT%frontend" && npx ng serve --port 4200"

echo.
echo Waiting for the frontend to finish compiling before opening your browser...
timeout /t 25 /nobreak >nul

start "" "http://localhost:4200"

echo.
echo ============================================
echo   All 4 services are running in their own windows.
echo   Login: admin / Admin@123
echo   To stop everything, close all 4 opened windows.
echo ============================================
echo.
pause
