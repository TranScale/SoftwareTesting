param(
    [string]$BaseUrl = "http://localhost:5068",
    [int]$DurationSeconds = 130,
    [string]$FfmpegPath = "C:\Users\Alucard30Dec\AppData\Local\Microsoft\WinGet\Links\ffmpeg.exe",
    [string]$OutputPath = "E:\Project\Online-Sales-Management-System\Report Test subject\video\OSMS-Automation-Demo.mp4"
)

$ErrorActionPreference = "Stop"

$repoRoot = "E:\Project\Online-Sales-Management-System"
$automationFolder = Join-Path $repoRoot "Report Test subject\automation"
$apiResultsFolder = Join-Path $repoRoot "Report Test subject\results\automation-api"
$resultsWorkbook = Join-Path $repoRoot "Report Test subject\results\OSMS-Final-Test-Results.xlsx"
$uiCommand = "powershell -ExecutionPolicy Bypass -File `"Report Test subject/automation/ui/run-ui-tests.ps1`" -BaseUrl `"$BaseUrl`" -Filter `"FullyQualifiedName~AdminLoginSmokeSucceeds`""
$apiCommand = "powershell -ExecutionPolicy Bypass -File `"Report Test subject/automation/api/newman/run-api-tests.ps1`" -BaseUrl `"$BaseUrl`""

if (-not (Test-Path $FfmpegPath)) {
    throw "ffmpeg not found at $FfmpegPath"
}

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Force
}

$ffmpegArgs = @(
    "-y",
    "-f", "gdigrab",
    "-framerate", "12",
    "-video_size", "1920x1080",
    "-i", "desktop",
    "-t", $DurationSeconds,
    "-c:v", "libx264",
    "-preset", "ultrafast",
    "-pix_fmt", "yuv420p",
    $OutputPath
)

$demoScript = @"
Set-Location '$repoRoot'
`$Host.UI.RawUI.WindowTitle = 'OSMS Automation Demo'
Write-Host 'Repository root: $repoRoot'
Write-Host ''
Write-Host 'UI automation command:'
Write-Host '$uiCommand'
Write-Host ''
& powershell -ExecutionPolicy Bypass -File 'Report Test subject/automation/ui/run-ui-tests.ps1' -BaseUrl '$BaseUrl' -Filter 'FullyQualifiedName~AdminLoginSmokeSucceeds'
Write-Host ''
Write-Host 'UI screenshot evidence: Report Test subject/evidence/ui/automation/20260406_053930_TC-UI-AUTH-001-success.png'
Write-Host ''
Write-Host 'API automation command:'
Write-Host '$apiCommand'
Write-Host ''
& powershell -ExecutionPolicy Bypass -File 'Report Test subject/automation/api/newman/run-api-tests.ps1' -BaseUrl '$BaseUrl'
Write-Host ''
Write-Host 'Opening API result folder and final results workbook...'
Start-Process explorer.exe '$apiResultsFolder' | Out-Null
Start-Sleep -Seconds 3
Start-Process '$resultsWorkbook' | Out-Null
Write-Host ''
Write-Host 'Automation demo finished. Keeping this window open until recording ends.'
Start-Sleep -Seconds 50
"@

$encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($demoScript))

$orchestrator = Start-Job -ScriptBlock {
    param($AutomationFolder, $Encoded)
    Start-Sleep -Seconds 3
    Start-Process explorer.exe $AutomationFolder | Out-Null
    Start-Sleep -Seconds 3
    Start-Process powershell.exe -ArgumentList "-NoLogo", "-NoProfile", "-EncodedCommand", $Encoded -WindowStyle Normal | Out-Null
} -ArgumentList $automationFolder, $encoded

& $FfmpegPath @ffmpegArgs | Out-Null

Wait-Job $orchestrator | Out-Null
Remove-Job $orchestrator -Force

if (-not (Test-Path $OutputPath)) {
    throw "Recording did not produce $OutputPath"
}
