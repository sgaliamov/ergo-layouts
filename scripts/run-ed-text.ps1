$ErrorActionPreference = "Stop"

Clear-Host

$currDir = (Get-Item -Path "./").FullName

For ($i = 1; $i -lt 1000; $i++) {
    Write-Host "### Run $i ###"
    .\publish\edText\edText.exe $currDir\samples 0.1 $currDir\publish\edText 4
}
