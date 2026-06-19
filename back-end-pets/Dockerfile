# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solution
COPY back-end-pets.sln ./

# Copiar proyectos (Clean Architecture)
COPY back-end-pets/back-end-pets.csproj back-end-pets/
COPY BackEndPets.Application/BackEndPets.Application.csproj BackEndPets.Application/
COPY BackEndPets.Domain/BackEndPets.Domain.csproj BackEndPets.Domain/
COPY BackEndPets.Infrastructure/BackEndPets.Infrastructure.csproj BackEndPets.Infrastructure/

# Restore
RUN dotnet restore back-end-pets/back-end-pets.csproj

# Copiar todo el código
COPY . .

# Publish
RUN dotnet publish back-end-pets/back-end-pets.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Healthcheck (opcional pero recomendado)
HEALTHCHECK --interval=10s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "back-end-pets.dll"]