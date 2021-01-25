$ErrorActionPreference = "Stop"

cls

$currDir = (Get-Item -Path "./").FullName

For ($i = 1; $i -lt 1000; $i++) {
    Write-Host "### Run $i ###"
    .\publish\edText.exe $currDir\samples 0.1 $currDir\publish 4
}
