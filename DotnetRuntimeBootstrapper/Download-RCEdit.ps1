$outputFilePath = "$PSScriptRoot/rcedit.exe"

# Check if already exists
if (Test-Path $outputFilePath) {
    Write-Host "Skipped downloading RCEdit, file already exists."
    exit
}

Write-Host "Downloading RCEdit..."

$url = "https://github.com/electron/rcedit/releases/download/v1.1.1/rcedit-x86.exe" # (x86 is universal)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$wc = New-Object System.Net.WebClient
$wc.DownloadFile($url, $outputFilePath)
$wc.Dispose()

Write-Host "Done downloading RCEdit."