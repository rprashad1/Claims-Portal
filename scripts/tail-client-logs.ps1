param(
    [int]$Tail = 50
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$logPath = Join-Path $scriptDir "..\logs\clientlogs.txt" | Resolve-Path -ErrorAction SilentlyContinue

if (-not $logPath) {
    Write-Host "Log file not found at expected location: scripts\..\logs\clientlogs.txt" -ForegroundColor Yellow
    Write-Host "Ensure the app is running and that client logs are being posted to /api/clientlogs." -ForegroundColor Yellow
    exit 1
}

Write-Host "Tailing client logs: $($logPath.Path) (press Ctrl+C to stop)" -ForegroundColor Green
Get-Content -Path $logPath.Path -Wait -Tail $Tail
