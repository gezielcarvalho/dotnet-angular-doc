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

# Change to the project root directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
Set-Location $projectRoot

# Use a fixed results directory to avoid stale files
$resultsDir = Join-Path -Path "backend.tests/TestResults" -ChildPath "coverage_run"
if (Test-Path $resultsDir) {
    Remove-Item -Recurse -Force $resultsDir
}
New-Item -ItemType Directory -Path $resultsDir | Out-Null

Write-Host "Running tests with coverage into $resultsDir ..."
& dotnet test backend.tests --nologo --collect:"XPlat Code Coverage" --results-directory "$resultsDir" --settings "backend.tests/coverlet.runsettings"
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

# Coverage color thresholds (customizable)
$GreenThreshold = 80  # Green for >= 80%
$YellowThreshold = 70 # Yellow for >= 70% and < 80%, Red for < 70%

# Per-file coverage
Write-Output ""
Write-Output "Per-file coverage:"
$packages = $xml.SelectNodes("//package")
$fileCoverage = @{}

foreach ($package in $packages) {
    $classes = $package.SelectNodes("classes/class")
    foreach ($class in $classes) {
        $fileName = $class.GetAttribute('filename')
        if ($fileName) {
            $lines = $class.SelectNodes("lines/line")
            $totalLines = 0
            $coveredLines = 0
            foreach ($line in $lines) {
                $hits = $line.GetAttribute('hits')
                if ($hits) {
                    [int]$hitCount = 0
                    [int]::TryParse($hits, [ref]$hitCount) | Out-Null
                    $totalLines++
                    if ($hitCount -gt 0) {
                        $coveredLines++
                    }
                }
            }

            if (-not $fileCoverage.ContainsKey($fileName)) {
                $fileCoverage[$fileName] = @{ Valid = 0; Covered = 0 }
            }
            $fileCoverage[$fileName].Valid += $totalLines
            $fileCoverage[$fileName].Covered += $coveredLines
        }
    }
}

# Create table data
$tableData = $fileCoverage.GetEnumerator() | 
    Where-Object { $_.Value.Valid -gt 0 } |
    ForEach-Object {
        $percent = [math]::Round(($_.Value.Covered / $_.Value.Valid) * 100, 2)
        [PSCustomObject]@{
            File = $_.Key
            Lines = "$($_.Value.Covered)/$($_.Value.Valid)"
            Coverage = $percent
        }
    } |
    Sort-Object -Property Coverage -Descending

# Display colored table
Write-Host ""
Write-Host "File".PadRight(60) -NoNewline
Write-Host "Lines".PadRight(10) -NoNewline
Write-Host "Coverage"
Write-Host ("-" * 60) -NoNewline
Write-Host ("-" * 10) -NoNewline
Write-Host ("-" * 8)

foreach ($row in $tableData) {
    $file = $row.File.PadRight(60)
    $lines = $row.Lines.PadRight(10)
    $coveragePercent = $row.Coverage
    
    # Determine color based on coverage
    if ($coveragePercent -ge $GreenThreshold) {
        $color = "Green"
    } elseif ($coveragePercent -ge $YellowThreshold) {
        $color = "Yellow"
    } else {
        $color = "Red"
    }
    
    Write-Host $file -NoNewline
    Write-Host $lines -NoNewline
    Write-Host "$coveragePercent%" -ForegroundColor $color
}

if($FailUnder -gt 0 -and $percent -lt $FailUnder){
  Write-Host "Coverage $percent% is below threshold $FailUnder% - failing" -ForegroundColor Red
  exit 1
}

exit 0
