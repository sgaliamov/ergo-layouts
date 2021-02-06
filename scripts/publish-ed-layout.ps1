$ErrorActionPreference = "Stop"

dotnet publish .\src\edLayout -o .\publish\edLayout -c release -r win-x64 -p:PublishSingleFile=true

New-Item .\publish\edLayout\layouts -ItemType Directory -Force | Out-Null
Copy-Item .\layouts\*.* -Destination .\publish\edLayout\layouts
