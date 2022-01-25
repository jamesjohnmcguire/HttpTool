CD %~dp0
CD ..\SourceCode

IF EXIST Bin\Release\AnyCPU\NUL DEL /Q Bin\Release\AnyCPU\*.*

CALL dotnet publish --configuration Release --runtime linux-x64 --self-contained true -p:PublishSingleFile=true -o Binaries\Linux HttpTool
CALL dotnet publish --configuration Release --runtime osx-x64 --self-contained true -p:PublishSingleFile=true -o Binaries\MacOS HttpTool
CALL dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishReadyToRun=true -p:PublishSingleFile=true --output Binaries\Windows HttpTool

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
