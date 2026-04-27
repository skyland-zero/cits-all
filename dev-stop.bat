@echo off
setlocal enableextensions

echo Stopping backend-api window...
taskkill /fi "WINDOWTITLE eq backend-api" /t /f >nul 2>nul
if errorlevel 1 (
  echo backend-api window not found or already stopped.
) else (
  echo backend-api stopped.
)

echo Stopping frontend-pc-v56 web-ele window...
taskkill /fi "WINDOWTITLE eq frontend-pc-v56 web-ele" /t /f >nul 2>nul
if errorlevel 1 (
  echo frontend-pc-v56 web-ele window not found or already stopped.
) else (
  echo frontend-pc-v56 web-ele stopped.
)

endlocal
