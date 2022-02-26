CD %~dp0
CD ..\SourceCode

IF EXIST Bin\Release\AnyCPU\NUL DEL /Q Bin\Release\AnyCPU\*.*

CALL dotnet publish --configuration Release -p:PublishSingleFile=true --runtime linux-x64 --self-contained true --output Binaries\Linux HttpTool
CALL dotnet publish --configuration Release -p:PublishSingleFile=true --runtime osx-x64 --self-contained true --output Binaries\MacOS HttpTool
CALL dotnet publish --configuration Release -p:PublishReadyToRun=true;PublishSingleFile=true --runtime win-x64 --self-contained true --output Binaries\Windows HttpTool

IF "%1"=="release" GOTO release
GOTO end

:release
CD Binaries\Linux
7z u HttpTool-Linux.zip .
MOVE HttpTool-Linux.zip ..

CD ..\MacOS
7z u HttpTool-MacOS.zip .
MOVE HttpTool-MacOS.zip ..

CD ..\Windows
7z u HttpTool-Windows.zip .
MOVE HttpTool-Windows.zip ..

CD ..
gh release create v%2 --notes %3 *.zip

:end
