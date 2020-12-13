CD %~dp0
CD ..

dotnet build --configuration Release

CD Bin\Release\x64

7z u HttpTool.zip . -xr!*.json -xr!ref

hub release create -a HttpTool.zip -m "%1" %1
