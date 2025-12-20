<#
.SYNOPSIS
  Run tests with code coverage and print a concise coverage summary to stdout.

.DESCRIPTION
  Runs `dotnet test` for the `backend.tests` project with the XPlat coverage
  collector. It locates the generated `coverage.cobertura.xml`, extracts the
  `line-rate` and prints a human-friendly summary. Optionally fails with exit
  code 1 when coverage is below a provided threshold (useful for CI pipelines).

.PARAMETER FailUnder
  Optional minimum acceptable line coverage percentage (e.g. 70). If the
  measured coverage is lower, the script will exit with code 1.

.EXAMPLE
  ./scripts/coverage.ps1
  ./scripts/coverage.ps1 -FailUnder 70
#>

param(
    [double]$FailUnder = 0
)

Set-StrictMode -Version Latest

Write-Host "Running tests with coverage..."
& dotnet test backend.tests --nologo --collect:"XPlat Code Coverage"
if($LASTEXITCODE -ne 0){
    Write-Error "dotnet test failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Find the cobertura XML
$cov = Get-ChildItem -Path "backend.tests/TestResults" -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if(-not $cov){
    Write-Error "coverage.cobertura.xml not found under backend.tests/TestResults"
    exit 2
}

[xml]$xml = Get-Content $cov.FullName
$root = $xml.DocumentElement
$lineRateAttr = $root.GetAttribute('line-rate')
if(-not $lineRateAttr){
    Write-Error "line-rate attribute not found in coverage XML"
    exit 3
}

[double]$lineRate = [double]$lineRateAttr
$linesCovered = $root.GetAttribute('lines-covered')
$linesValid = $root.GetAttribute('lines-valid')
$percent = [math]::Round($lineRate * 100, 2)

Write-Output "Coverage summary:"
Write-Output "  Line coverage : $percent% ($linesCovered / $linesValid)"

if($FailUnder -gt 0 -and $percent -lt $FailUnder){
    Write-Host "Coverage $percent% is below threshold $FailUnder% - failing" -ForegroundColor Red
    exit 1
}

exit 0
