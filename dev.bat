@echo off
setlocal enableextensions

set "ROOT_DIR=%~dp0"
set "BACKEND_DIR=%ROOT_DIR%backend-api"
set "FRONTEND_DIR=%ROOT_DIR%frontend-pc"

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERROR] dotnet not found in PATH.
  exit /b 1
)

where pnpm >nul 2>nul
if errorlevel 1 (
  echo [ERROR] pnpm not found in PATH.
  exit /b 1
)

if not exist "%BACKEND_DIR%\src\MyApi.HttpApi\MyApi.HttpApi.csproj" (
  echo [ERROR] Backend project not found: %BACKEND_DIR%
  exit /b 1
)

if not exist "%FRONTEND_DIR%\package.json" (
  echo [ERROR] Frontend project not found: %FRONTEND_DIR%
  exit /b 1
)

echo Starting backend-api on http://localhost:5120 ...
start "backend-api" cmd /k "cd /d "%BACKEND_DIR%" && dotnet watch run --launch-profile http --project "src\MyApi.HttpApi\MyApi.HttpApi.csproj""

echo Starting frontend-pc web-ele on http://localhost:5777 ...
start "frontend-pc web-ele" cmd /k "cd /d "%FRONTEND_DIR%" && pnpm dev:ele"

echo.
echo Backend:  http://localhost:5120
echo Frontend: http://localhost:5777
echo.
echo The frontend proxies /api to the backend in development.

endlocal
