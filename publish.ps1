# MIDIFlux Publish Script
# Builds and publishes MIDIFlux to ./publish folder

param(
    [switch]$FrameworkDependent = $false
)

$ErrorActionPreference = "Stop"

$publishDir = "$PSScriptRoot\publish"
$projectPath = "$PSScriptRoot\src\MIDIFlux.App\MIDIFlux.App.csproj"

if (-not (Test-Path $projectPath)) {
    Write-Host "Error: Project file not found at $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "=== MIDIFlux Publish Script ===" -ForegroundColor Cyan
Write-Host ""

# Clean publish directory
if (Test-Path $publishDir) {
    Write-Host "Cleaning existing publish directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $publishDir
}

New-Item -ItemType Directory -Path $publishDir | Out-Null

# Build publish arguments
$publishArgs = @(
    "publish"
    $projectPath
    "-c", "Release"
    "-o", $publishDir
    "-r", "win-x64"
)

if ($FrameworkDependent) {
    $publishArgs += "--self-contained", "false"
    Write-Host "Mode: Framework-dependent (requires .NET 10.0 runtime)" -ForegroundColor Green
} else {
    $publishArgs += "--self-contained", "true"
    $publishArgs += "-p:PublishSingleFile=true"
    Write-Host "Mode: Self-contained single-file executable" -ForegroundColor Green
}

Write-Host ""
Write-Host "Publishing MIDIFlux..." -ForegroundColor Cyan
Write-Host "Command: dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray
Write-Host ""

& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Publish completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Output directory: $publishDir" -ForegroundColor Cyan

# List main files
Write-Host ""
Write-Host "Published files:" -ForegroundColor Cyan
Get-ChildItem $publishDir -File | ForEach-Object {
    $size = "{0:N2} MB" -f ($_.Length / 1MB)
    Write-Host "  $($_.Name) ($size)" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Usage ===" -ForegroundColor Yellow
Write-Host "  .\publish.ps1                    # Self-contained single-file exe (default)"
Write-Host "  .\publish.ps1 -FrameworkDependent # Framework-dependent build (requires .NET 10)"
Write-Host ""

