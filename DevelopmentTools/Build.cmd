CD %~dp0
CD ..\SourceCode

IF EXIST Binaries\NUL RD /S /Q Binaries

dotnet publish --configuration Release --output Binaries\Windows -p:PublishReadyToRun=true -p:PublishSingleFile=true --runtime win-x64 --self-contained HttpTool

IF "%1"=="release" GOTO release
GOTO end

:release
dotnet publish --configuration Release --output Binaries\Linux -p:PublishReadyToRun=true -p:PublishSingleFile=true --runtime linux-x64 --self-contained HttpTool

dotnet publish --configuration Release --output Binaries\MacOS -p:PublishReadyToRun=true -p:PublishSingleFile=true --runtime osx-x64 --self-contained HttpTool

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
gh release create v%2 --notes %2 *.zip

:end
