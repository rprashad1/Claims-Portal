$content = Get-Content 'wwwroot\templates\BI SIGNED LETTER.htm' -Raw
Set-Content -Path rendered_response_direct.html -Value $content -Encoding utf8
Write-Output "WROTE rendered_response_direct.html SIZE: $($content.Length)"
Write-Output "---SNIPPET---"
if ($content.Length -ge 500) { $content.Substring(0,500) } else { $content }
