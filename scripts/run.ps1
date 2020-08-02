$ErrorActionPreference = "Stop"

cls

$currDir = (Get-Item -Path "./").FullName

$TextInfo = (Get-Culture).TextInfo

Get-ChildItem $currDir\publish\layouts\*.json -Recurse | ForEach-Object {
    Write-Host "## $($TextInfo.ToTitleCase($_.Basename))"
    Write-Host
    Write-Host "`````` ini"
    .\publish\edLayout.exe -i $currDir\samples -l $_ -sp false -o .\publish\results.csv
    Write-Host "``````"
    Write-Host
}
