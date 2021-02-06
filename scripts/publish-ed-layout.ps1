$ErrorActionPreference = "Stop"

Get-ChildItem -Include "bin", "obj", "publish" -Recurse | Remove-Item -Force -Recurse

dotnet publish .\src\edLayout -o .\publish -c release -r win-x64 -p:PublishSingleFile=true

New-Item .\publish\layouts -ItemType Directory -Force | Out-Null
Copy-Item .\layouts\*.* -Destination .\publish\layouts -Recurse
