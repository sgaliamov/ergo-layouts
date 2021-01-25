$ErrorActionPreference = "Stop"

Get-ChildItem -Include "bin", "obj", "publish" -Recurse | Remove-Item -Force -Recurse

dotnet publish .\src\edText -o .\publish -c release -r win-x64 -p:PublishSingleFile=true
