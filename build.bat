:: Build all AAS API Servers
::

@echo off

:: Build AASX File Server

dotnet restore "src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj"
dotnet build "src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj" -c Debug
echo Now, run the following to start the project: dotnet run -p "src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj" --launch-profile web.
echo.

docker build -f "src\aas-api-webapp-aasxfile\Dockerfile" --force-rm -t aas-aasxfile-server:latest .
