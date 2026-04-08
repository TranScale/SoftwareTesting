param(
    [string]$BaseUrl = "http://localhost:5068",
    [string]$Browser = "chrome",
    [switch]$Headless,
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"

$uiRoot = $PSScriptRoot
$project = Join-Path $uiRoot "OSMS.UITests\OSMS.UITests.csproj"
$resultsRoot = Join-Path (Resolve-Path (Join-Path $uiRoot "..\..\results")).Path "automation-ui"

New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null

$env:OSMS_UI_BASE_URL = $BaseUrl
$env:OSMS_UI_BROWSER = $Browser
$env:OSMS_UI_HEADLESS = if ($Headless.IsPresent) { "true" } else { "false" }

$arguments = @(
    "test",
    $project,
    "--results-directory",
    $resultsRoot,
    "--logger",
    "trx;LogFileName=ui-tests.trx"
)

if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $arguments += @("--filter", $Filter)
}

& dotnet @arguments
