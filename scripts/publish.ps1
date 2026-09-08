$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

dotnet publish src/ForzaTelemetryHaptics.App/ForzaTelemetryHaptics.App.csproj `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o publish

Copy-Item -Force config.json publish/config.json
Write-Host "Published to $root\publish\ForzaTelemetryHaptics.exe"
