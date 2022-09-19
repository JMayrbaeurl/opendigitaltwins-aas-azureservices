#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/aas-api-webapp-registry/AAS WebApp Registry.csproj", "src/aas-api-webapp-registry/"]
COPY ["src/aas-registry-service/AAS Registry Service.csproj", "src/aas-registry-service/"]
COPY ["src/aas-api-models/AAS API Models.csproj", "src/aas-api-models/"]
COPY ["src/aas-services-support/AAS Services Support.csproj", "src/aas-services-support/"]
RUN dotnet restore "src/aas-api-webapp-registry/AAS WebApp Registry.csproj"
COPY . .
WORKDIR "/src/src/aas-api-webapp-registry"
RUN dotnet build "AAS WebApp Registry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AAS WebApp Registry.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AAS WebApp Registry.dll"]