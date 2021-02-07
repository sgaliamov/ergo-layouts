$ErrorActionPreference = "Stop"

Clear-Host

$currDir = (Get-Item -Path "./").FullName

$TextInfo = (Get-Culture).TextInfo

Get-ChildItem $currDir\publish\edLayout\layouts\*.json | ForEach-Object {
    Write-Host "## $($TextInfo.ToTitleCase($_.Basename))"
    Write-Host
    Write-Host "`````` ini"
    .\publish\edLayout\edLayout.exe -i $currDir\samples -l $_ -sp false -o .\publish\edLayout\results.csv
    Write-Host "``````"
    Write-Host
}
