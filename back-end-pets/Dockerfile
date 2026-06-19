# ===== BASE BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# COPY CSPROJ FIRST (CACHE RESTORE)
COPY back-end-pets/back-end-pets.csproj back-end-pets/
COPY BackEndPets.Application/*.csproj BackEndPets.Application/
COPY BackEndPets.Domain/*.csproj BackEndPets.Domain/
COPY BackEndPets.Infraestructure/*.csproj BackEndPets.Infraestructure/

RUN dotnet restore back-end-pets/back-end-pets.csproj

# COPY EVERYTHING
COPY . .

RUN dotnet publish back-end-pets/back-end-pets.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "back-end-pets.dll"]