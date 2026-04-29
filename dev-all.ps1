$ErrorActionPreference = "Stop"

$RootDir = $PSScriptRoot
$BackendDir = Join-Path $RootDir "backend-api"
$FrontendDir = Join-Path $RootDir "frontend-pc"

if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "[ERROR] dotnet not found in PATH."
    exit 1
}

if (!(Get-Command pnpm -ErrorAction SilentlyContinue)) {
    Write-Error "[ERROR] pnpm not found in PATH."
    exit 1
}

Write-Host "Starting backend-api in background..." -ForegroundColor Green
$BackendProcess = Start-Process -FilePath "dotnet" -ArgumentList "watch run --launch-profile http --project src\MyApi.HttpApi\MyApi.HttpApi.csproj" -WorkingDirectory $BackendDir -PassThru -NoNewWindow

Write-Host "Starting frontend-pc in background..." -ForegroundColor Green
# Using cmd /c for pnpm since it's a batch/cmd wrapper on Windows
$FrontendProcess = Start-Process -FilePath "cmd" -ArgumentList "/c pnpm dev:ele" -WorkingDirectory $FrontendDir -PassThru -NoNewWindow

Write-Host "`nDevelopment servers are running:`nBackend:  http://localhost:5120`nFrontend: http://localhost:5777`n" -ForegroundColor Cyan
Write-Host "Press CTRL+C or type 'exit' to stop the servers...`n" -ForegroundColor Yellow

try {
    # Keep script running to maintain processes
    Wait-Process -Id $BackendProcess.Id, $FrontendProcess.Id -ErrorAction SilentlyContinue
}
finally {
    Write-Host "`nStopping servers..." -ForegroundColor Yellow
    if (!$BackendProcess.HasExited) { Stop-Process -Id $BackendProcess.Id -Force -ErrorAction SilentlyContinue }
    if (!$FrontendProcess.HasExited) { Stop-Process -Id $FrontendProcess.Id -Force -ErrorAction SilentlyContinue }
    Write-Host "Servers stopped." -ForegroundColor Green
}
