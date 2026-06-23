@echo off
setlocal

set "APP_DIR=%~dp0"
set "API_URL=http://10.36.0.4:5000"

echo.
echo VETTI Flow API
echo URL: %API_URL%
echo.

netsh advfirewall firewall add rule name="VETTI Flow API 5000" dir=in action=allow protocol=TCP localport=5000 >nul 2>nul

cd /d "%APP_DIR%"

if exist "VettiFlow.Api.exe" (
  set "ASPNETCORE_ENVIRONMENT=Production"
  set "VettiFlow__ListenUrl=%API_URL%"
  VettiFlow.Api.exe
  goto :eof
)

if exist "VettiFlow.Api.dll" (
  set "ASPNETCORE_ENVIRONMENT=Production"
  set "VettiFlow__ListenUrl=%API_URL%"
  dotnet VettiFlow.Api.dll
  goto :eof
)

echo Nao encontrei VettiFlow.Api.exe nem VettiFlow.Api.dll nesta pasta.
echo Rode o publish antes ou copie este arquivo para a pasta publicada da API.
pause
