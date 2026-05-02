# =====================================================
# Stage 1: Build
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos primero los .csproj para aprovechar la caché de capas
# en restauraciones de paquetes cuando no cambian dependencias.
COPY ["FishClubAlginet.API/FishClubAlginet.API.csproj",                       "FishClubAlginet.API/"]
COPY ["FishClubAlginet.Application/FishClubAlginet.Application.csproj",       "FishClubAlginet.Application/"]
COPY ["FishClubAlginet.Core/FishClubAlginet.Core.csproj",                     "FishClubAlginet.Core/"]
COPY ["FishClubAlginet.Infrastructure/FishClubAlginet.Infrastructure.csproj", "FishClubAlginet.Infrastructure/"]
COPY ["FishClubAlginet.Contracts/FishClubAlginet.Contracts.csproj",           "FishClubAlginet.Contracts/"]

RUN dotnet restore "FishClubAlginet.API/FishClubAlginet.API.csproj"

# Ahora copiamos el resto del código fuente
COPY FishClubAlginet.API/             FishClubAlginet.API/
COPY FishClubAlginet.Application/     FishClubAlginet.Application/
COPY FishClubAlginet.Core/            FishClubAlginet.Core/
COPY FishClubAlginet.Infrastructure/  FishClubAlginet.Infrastructure/
COPY FishClubAlginet.Contracts/       FishClubAlginet.Contracts/

WORKDIR /src/FishClubAlginet.API
RUN dotnet publish "FishClubAlginet.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =====================================================
# Stage 2: Runtime
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Variables por defecto (override desde docker-compose / .env)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FishClubAlginet.API.dll"]
