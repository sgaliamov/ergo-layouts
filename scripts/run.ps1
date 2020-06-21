$ErrorActionPreference = "Stop"

$currDir = (Get-Item -Path "./").FullName
.\publish\edLayout.exe $currDir\samples *.txt false $currDir\layouts\qwerty.json