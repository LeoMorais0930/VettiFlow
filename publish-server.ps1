param(
    [string]$OutputPath = "..\dist\VettiFlow.Api-server-win-x64",
    [string]$ZipPath = "..\dist\VettiFlow.Api-server-win-x64-10.36.0.4.zip"
)

$ErrorActionPreference = "Stop"

Write-Host "Publicando VETTI Flow API para o servidor 10.36.0.4..." -ForegroundColor Cyan

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

dotnet publish .\VettiFlow.Api.csproj -c Release -r win-x64 --self-contained true -o $OutputPath

Copy-Item .\run-vettiflow-server.bat -Destination (Join-Path $OutputPath "run-vettiflow-server.bat") -Force

if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

$zipDir = Split-Path $ZipPath -Parent
if ($zipDir -and -not (Test-Path $zipDir)) {
    New-Item -ItemType Directory -Path $zipDir | Out-Null
}

Compress-Archive -Path (Join-Path $OutputPath "*") -DestinationPath $ZipPath -Force

Write-Host ""
Write-Host "Pronto." -ForegroundColor Green
Write-Host "Pasta publicada: $OutputPath"
Write-Host "ZIP para subir no servidor: $ZipPath"
Write-Host ""
Write-Host "No servidor, extraia o ZIP e execute run-vettiflow-server.bat como administrador."
