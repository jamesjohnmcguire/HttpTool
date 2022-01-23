CD %~dp0
CD ..\SourceCode

IF EXIST Bin\Release\AnyCPU\NUL DEL /Q Bin\Release\AnyCPU\*.*

CALL dotnet publish -c Release --runtime linux-x64 --self-contained true -p:PublishSingleFile=true -o Binaries\Linux HttpTool
CALL dotnet publish -c Release --runtime osx-x64 --self-contained true -p:PublishSingleFile=true -o Binaries\MacOS HttpTool
CALL dotnet publish -c Release --runtime win-x64 --self-contained true -p:PublishReadyToRun=true -p:PublishSingleFile=true --output Binaries\Windows HttpTool

IF "%1"=="release" GOTO release
GOTO end

:release
CD Bin\Release\x64

7z u HttpTool.zip . -xr!*.json -xr!ref

hub release create -a HttpTool.zip -m "%2" v%2

:end