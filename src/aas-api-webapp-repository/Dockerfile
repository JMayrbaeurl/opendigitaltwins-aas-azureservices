#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/aas-api-webapp-repository/AAS WebApp Repository.csproj", "src/aas-api-webapp-repository/"]
COPY ["src/aas-api-repository-adt/AasRepositoryAdt.csproj", "src/aas-api-repository-adt/"]
COPY ["src/AAS ADT SDK/AAS ADT SDK.csproj", "src/AAS ADT SDK/"]
COPY ["src/aas-services-support/AAS Services Support.csproj", "src/aas-services-support/"]
COPY ["src/aas-api-models/AAS API Models.csproj", "src/aas-api-models/"]
COPY ["src/aas-repository/AAS Repository.csproj", "src/aas-repository/"]
RUN dotnet restore "src/aas-api-webapp-repository/AAS WebApp Repository.csproj"
COPY . .
WORKDIR "/src/src/aas-api-webapp-repository"
RUN dotnet build "AAS WebApp Repository.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AAS WebApp Repository.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AASWebAppRepository.dll"]