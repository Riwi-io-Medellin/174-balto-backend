FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# COPY SOLUTION FIRST
COPY back-end-pets.sln ./

# COPY PROJECTS (RUTAS CORRECTAS)
COPY back-end-pets/back-end-pets.csproj back-end-pets/
COPY BackEndPets.Application/BackEndPets.Application.csproj BackEndPets.Application/
COPY BackEndPets.Domain/BackEndPets.Domain.csproj BackEndPets.Domain/
COPY BackEndPets.Infraestructure/BackEndPets.Infraestructure.csproj BackEndPets.Infraestructure/

RUN dotnet restore back-end-pets/back-end-pets.csproj

# COPY EVERYTHING
COPY . .

RUN dotnet publish back-end-pets/back-end-pets.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "back-end-pets.dll"]