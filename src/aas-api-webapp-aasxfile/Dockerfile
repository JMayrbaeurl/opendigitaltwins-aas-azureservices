#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj", "src/aas-api-webapp-aasxfile/"]
RUN dotnet restore "src/aas-api-webapp-aasxfile/AAS WebApp AASX File Server.csproj"
COPY . .
WORKDIR "/src/src/aas-api-webapp-aasxfile"
RUN dotnet build "AAS WebApp AASX File Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AAS WebApp AASX File Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AAS WebApp AASX File Server.dll"]