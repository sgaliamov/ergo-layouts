$ErrorActionPreference = "Stop"

cls

$currDir = (Get-Item -Path "./").FullName

Get-ChildItem $currDir\publish\layouts\*.json -Recurse | ForEach-Object {
    Write-Host $_
    .\publish\edLayout.exe $currDir\samples *.txt false $_
    Write-Host
    Write-Host
}
