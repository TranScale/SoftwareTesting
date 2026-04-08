param(
    [string]$BaseUrl = "http://localhost:5068"
)

$ErrorActionPreference = "Stop"

$apiRoot = Split-Path -Parent $PSScriptRoot
$collection = Join-Path $apiRoot "postman\collections\OSMS-API-Automation.postman_collection.json"
$environment = Join-Path $apiRoot "postman\environments\OSMS-Local.postman_environment.json"
$resultsRoot = Join-Path (Resolve-Path (Join-Path $apiRoot "..\..\results")).Path "automation-api"
$junitPath = Join-Path $resultsRoot "newman-results.xml"

New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null

& npx newman run $collection `
    -e $environment `
    --env-var "baseUrl=$BaseUrl" `
    --reporters "cli,junit" `
    --reporter-junit-export $junitPath
