Set-Location 'C:\Projects\ClaimsPortal\publish'
# Start published app (background)
Start-Process -FilePath 'dotnet' -ArgumentList 'ClaimsPortal.dll' -WorkingDirectory (Get-Location) -WindowStyle Hidden -PassThru | Out-Null
Start-Sleep -Seconds 2
Write-Output "Started published app (attempted)"

# Check CSS
try {
    $r = Invoke-WebRequest 'http://localhost:5000/ClaimsPortal.styles.css' -UseBasicParsing -TimeoutSec 10
    Write-Output "CSS HTTP status: $($r.StatusCode)"
    Write-Output "CSS length: $($r.RawContentLength)"
} catch {
    Write-Output "CSS HTTP failed: $($_.Exception.Message)"
}

# Check Blazor script
try {
    $r2 = Invoke-WebRequest 'http://localhost:5000/_framework/blazor.web.js' -UseBasicParsing -TimeoutSec 10
    Write-Output "BlazorJS HTTP status: $($r2.StatusCode)"
    Write-Output "BlazorJS length: $($r2.RawContentLength)"
} catch {
    Write-Output "BlazorJS HTTP failed: $($_.Exception.Message)"
}
