@echo off
setlocal enableextensions

echo Stopping backend-api window...
taskkill /fi "WINDOWTITLE eq backend-api" /t /f >nul 2>nul
if errorlevel 1 (
  echo backend-api window not found or already stopped.
) else (
  echo backend-api stopped.
)

echo Stopping frontend-pc web-ele window...
taskkill /fi "WINDOWTITLE eq frontend-pc web-ele" /t /f >nul 2>nul
if errorlevel 1 (
  echo frontend-pc web-ele window not found or already stopped.
) else (
  echo frontend-pc web-ele stopped.
)

endlocal
