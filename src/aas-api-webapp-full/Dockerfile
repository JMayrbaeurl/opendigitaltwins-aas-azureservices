#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/aas-api-webapp-full/AAS WebApp full.csproj", "src/aas-api-webapp-full/"]
RUN dotnet restore "src/aas-api-webapp-full/AAS WebApp full.csproj"
COPY . .
WORKDIR "/src/src/aas-api-webapp-full"
RUN dotnet build "AAS WebApp full.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AAS WebApp full.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AASAPIServerFull.dll"]