#!/usr/bin/env bash
#
# Build all AAS API Servers
#

dotnet restore "src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj" && \
    dotnet build "src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj" -c Debug && \
    echo "Now, run the following to start the project: dotnet run -p 'src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj' --launch-profile web"
	
docker build -f "src/aas-api-webapp-aasxfile/Dockerfile" --force-rm -t aas-aasxfile-server:latest . && \
	echo "Now, run with: docker run -d -p 4567:80 aas-aasxfile-server:latest"
