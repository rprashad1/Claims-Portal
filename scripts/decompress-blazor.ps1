$comp = Get-ChildItem 'obj\Debug\net10.0\compressed' -Filter '*j8lzlu28q6*.gz' | Select-Object -First 1
if (-not $comp) {
    Write-Host 'Compressed file not found'
    exit 1
}
$src = $comp.FullName
$dest_dir = 'wwwroot\_framework'
if (-not (Test-Path $dest_dir)) { New-Item -ItemType Directory -Path $dest_dir | Out-Null }
$dest = Join-Path $dest_dir 'blazor.web.js'
try {
    $fsin = [IO.File]::OpenRead($src)
    $fsout = [IO.File]::Create($dest)
    $gzip = New-Object IO.Compression.GzipStream($fsin, [IO.Compression.CompressionMode]::Decompress)
    $gzip.CopyTo($fsout)
    $gzip.Close()
    $fsout.Close()
    $fsin.Close()
    Write-Host "Decompressed to $dest"
} catch {
    Write-Host "Decompression failed: $($_.Exception.Message)"
    exit 1
}
