CD %~dp0
CD ..\SourceCode

IF EXIST Bin\Release\x64\NUL DEL /Q Bin\Release\x64\*.*

dotnet build --configuration Release

IF "%1"=="release" GOTO release
GOTO end

:release
CD Bin\Release\x64

7z u HttpTool.zip . -xr!*.json -xr!ref

hub release create -a HttpTool.zip -m "%2" v%2

:end