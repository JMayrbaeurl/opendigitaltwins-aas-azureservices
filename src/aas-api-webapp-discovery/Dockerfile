#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/aas-api-webapp-discovery/AAS WebApp Discovery.csproj", "src/aas-api-webapp-discovery/"]
RUN dotnet restore "src/aas-api-webapp-discovery/AAS WebApp Discovery.csproj"
COPY . .
WORKDIR "/src/src/aas-api-webapp-discovery"
RUN dotnet build "AAS WebApp Discovery.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AAS WebApp Discovery.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AAS WebApp Discovery.dll"]