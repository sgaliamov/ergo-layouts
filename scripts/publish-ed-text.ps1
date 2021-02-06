$ErrorActionPreference = "Stop"

dotnet publish .\src\edText -o .\publish\edText -c release -r win-x64 -p:PublishSingleFile=true
