# MIDIFlux Test Runner Script
# Runs unit and integration tests for MIDIFlux.Core

param(
    [string]$Filter = "",
    [switch]$Coverage = $false,
    [switch]$Verbose = $false,
    [switch]$Watch = $false,
    [string]$Output = "normal"
)

Write-Host "MIDIFlux Test Runner" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green

# Set the test project path
$TestProject = "src\MIDIFlux.Core.Tests\MIDIFlux.Core.Tests.csproj"

# Check if test project exists
if (-not (Test-Path $TestProject)) {
    Write-Host "Error: Test project not found at $TestProject" -ForegroundColor Red
    exit 1
}

# Build the base command
$Command = "dotnet test `"$TestProject`""

# Add verbosity
if ($Verbose) {
    $Command += " --verbosity detailed"
} else {
    $Command += " --verbosity $Output"
}

# Add filter if specified
if ($Filter) {
    $Command += " --filter `"$Filter`""
    Write-Host "Running tests with filter: $Filter" -ForegroundColor Yellow
}

# Add coverage if requested
if ($Coverage) {
    $Command += " --collect:`"XPlat Code Coverage`""
    Write-Host "Code coverage collection enabled" -ForegroundColor Yellow
}

# Add watch mode if requested
if ($Watch) {
    $Command += " --watch"
    Write-Host "Watch mode enabled - tests will re-run on file changes" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Executing: $Command" -ForegroundColor Cyan
Write-Host ""

# Execute the command
try {
    Invoke-Expression $Command
    $ExitCode = $LASTEXITCODE
    
    if ($ExitCode -eq 0) {
        Write-Host ""
        Write-Host "All tests passed successfully!" -ForegroundColor Green
        
        if ($Coverage) {
            Write-Host ""
            Write-Host "Coverage reports generated in TestResults directory" -ForegroundColor Yellow
            Write-Host "To view coverage report, install reportgenerator:" -ForegroundColor Yellow
            Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Gray
            Write-Host "Then run:" -ForegroundColor Yellow
            Write-Host "  reportgenerator -reports:TestResults\*\coverage.cobertura.xml -targetdir:TestResults\CoverageReport" -ForegroundColor Gray
        }
    } else {
        Write-Host ""
        Write-Host "Some tests failed. Exit code: $ExitCode" -ForegroundColor Red
    }
} catch {
    Write-Host "Error running tests: $_" -ForegroundColor Red
    $ExitCode = 1
}

Write-Host ""
Write-Host "Test run completed." -ForegroundColor Green

# Examples of usage
if ($ExitCode -eq 0 -and -not $Filter -and -not $Coverage -and -not $Watch) {
    Write-Host ""
    Write-Host "Example usage:" -ForegroundColor Cyan
    Write-Host "  .\run-tests.ps1 -Filter `"MidiActionEngineTests`"     # Run specific test class" -ForegroundColor Gray
    Write-Host "  .\run-tests.ps1 -Coverage                           # Run with code coverage" -ForegroundColor Gray
    Write-Host "  .\run-tests.ps1 -Verbose                            # Run with detailed output" -ForegroundColor Gray
    Write-Host "  .\run-tests.ps1 -Watch                              # Run in watch mode" -ForegroundColor Gray
    Write-Host "  .\run-tests.ps1 -Filter `"Integration`" -Verbose     # Run integration tests with details" -ForegroundColor Gray
}

exit $ExitCode
