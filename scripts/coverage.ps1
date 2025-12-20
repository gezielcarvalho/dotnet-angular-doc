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

# Use a timestamped results directory to avoid stale files
$timestamp = [int][double]::Parse((Get-Date -UFormat %s))
$resultsDir = Join-Path -Path "backend.tests/TestResults" -ChildPath "run_$timestamp"
New-Item -ItemType Directory -Path $resultsDir | Out-Null

Write-Host "Running tests with coverage into $resultsDir ..."
& dotnet test backend.tests --nologo --collect:"XPlat Code Coverage" --results-directory "$resultsDir"
if($LASTEXITCODE -ne 0){
  Write-Error "dotnet test failed with exit code $LASTEXITCODE"
  exit $LASTEXITCODE
}

# Find the most recent cobertura XML in the results dir (with a small retry if empty)
function Get-CoverageFile($dir){
  Get-ChildItem -Path $dir -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
}

$cov = Get-CoverageFile $resultsDir
if(-not $cov){
  Write-Error "coverage.cobertura.xml not found under $resultsDir"
  exit 2
}
if(-not $cov){
    Write-Error "coverage.cobertura.xml not found under backend.tests/TestResults"
    exit 2
}


[xml]$xml = Get-Content $cov.FullName
$root = $xml.DocumentElement

# Safely parse integer attributes (avoid PowerShell '??' which isn't supported everywhere)
$linesValid = 0
$linesCovered = 0
$attr = $root.GetAttribute('lines-valid')
if(-not [string]::IsNullOrEmpty($attr)){
  [int]::TryParse($attr, [ref]$linesValid) | Out-Null
}
$attr = $root.GetAttribute('lines-covered')
if(-not [string]::IsNullOrEmpty($attr)){
  [int]::TryParse($attr, [ref]$linesCovered) | Out-Null
}

if($linesValid -eq 0){
  # try a short retry in case collector wrote placeholder
  Start-Sleep -Seconds 1
  $cov = Get-CoverageFile $resultsDir
  if(-not $cov){
    Write-Error "coverage.cobertura.xml disappeared under $resultsDir"
    exit 2
  }
  [xml]$xml = Get-Content $cov.FullName
  $root = $xml.DocumentElement
  $linesValid = 0
  $linesCovered = 0
  $attr = $root.GetAttribute('lines-valid')
  if(-not [string]::IsNullOrEmpty($attr)){
    [int]::TryParse($attr, [ref]$linesValid) | Out-Null
  }
  $attr = $root.GetAttribute('lines-covered')
  if(-not [string]::IsNullOrEmpty($attr)){
    [int]::TryParse($attr, [ref]$linesCovered) | Out-Null
  }
}

Write-Output "Coverage file: $($cov.FullName)"
if($linesValid -eq 0){
  Write-Error "Coverage file has zero valid lines (lines-valid=0); failing"
  exit 4
}

# Compute percentage robustly
$lineRateAttr = $root.GetAttribute('line-rate')
[double]$lineRate = 0
if($lineRateAttr){
  [double]::TryParse($lineRateAttr, [ref]$lineRate) | Out-Null
}
$percent = [math]::Round($lineRate * 100, 2)

Write-Output "Coverage summary:"
Write-Output "  Line coverage : $percent% ($linesCovered / $linesValid)"

if($FailUnder -gt 0 -and $percent -lt $FailUnder){
  Write-Host "Coverage $percent% is below threshold $FailUnder% - failing" -ForegroundColor Red
  exit 1
}

exit 0
