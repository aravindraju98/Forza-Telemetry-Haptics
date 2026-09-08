@echo off
setlocal
set DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe
if not exist "%DOTNET%" set DOTNET=dotnet
"%DOTNET%" build "%~dp0src\ForzaTelemetryHaptics.App\ForzaTelemetryHaptics.App.csproj" -c Release --nologo
if errorlevel 1 exit /b 1
"%DOTNET%" run --project "%~dp0src\ForzaTelemetryHaptics.App\ForzaTelemetryHaptics.App.csproj" -c Release --no-build
