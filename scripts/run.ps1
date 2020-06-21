$ErrorActionPreference = "Stop"

cls

$currDir = (Get-Item -Path "./").FullName

Get-ChildItem $currDir\publish\layouts\*.json -Recurse | ForEach-Object {
    Write-Host
    Write-Host
    Write-Host $_
    .\publish\edLayout.exe $currDir\samples *.txt false $_
}
