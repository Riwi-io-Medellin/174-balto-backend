# Balto — Backend API

API REST en .NET 10 para la plataforma Balto. Conecta dueños de mascotas, walkers y negocios pet care.

## Stack

- **.NET 10** — Minimal API (sin controllers)
- **PostgreSQL** — base de datos principal
- **Entity Framework Core** + Npgsql
- **SignalR** — seguimiento en tiempo real (paseos GPS)

## Arquitectura — Clean Architecture

```
back-end-pets.sln
│
├── BackEndPets.Domain/          # núcleo — sin dependencias externas
│   ├── Entities/                # modelos de dominio (User, Pet, Walker...)
│   ├── Interfaces/              # contratos de repositorios
│   └── Common/                  # base classes, value objects
│
├── BackEndPets.Application/     # lógica de negocio
│   ├── DTOs/                    # objetos request/response por módulo
│   ├── Interfaces/              # contratos de servicios de aplicación
│   ├── Services/                # implementación de lógica de negocio
│   └── UseCases/                # casos de uso organizados por módulo
│
├── BackEndPets.Infrastructure/  # acceso a datos y servicios externos
│   ├── Data/                    # AppDbContext, configuraciones EF Core
│   └── Repositories/            # implementación de repositorios con EF Core
│
└── back-end-pets/ (API)         # capa de entrada HTTP
    ├── Endpoints/               # endpoints agrupados por módulo
    ├── Extensions/              # registro de DI, middleware
    └── Program.cs               # entry point
```

### Regla de dependencias

```
API → Application → Domain ← Infrastructure
```

Cada capa solo conoce la capa interior. `Domain` no importa nada externo.

## Setup local

### Requisitos

- .NET 10 SDK
- PostgreSQL corriendo
- Variables de entorno configuradas

### Variables de entorno

Crear `back-end-pets/appsettings.Development.json` (no se commitea):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pets_db;Username=postgres_pets;Password=TU_PASSWORD"
  }
}
```

### Correr

```bash
dotnet run --project back-end-pets/back-end-pets.csproj
```

API disponible en `http://localhost:5001`. Documentación OpenAPI en `http://localhost:5001/openapi/v1.json` (solo en Development).

## Paquetes

| Paquete | Uso |
|---------|-----|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Registro, login y gestión de usuarios |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Valida el token en cada petición protegida |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Conecta la API con PostgreSQL |
| `Microsoft.EntityFrameworkCore.Design` | Crea y aplica cambios en la base de datos |

## Flujo de ramas

```
feature/* → dev → staging → main
```

- `feature/*` siempre sale de `dev`
- Nunca commitear directo a `main` o `staging`
- Merge a `main` solo cuando `staging` está validado

## Convenciones de API

- Base: `/api/{recurso}` — sin versionado
- Casing: `kebab-case` → `/api/walking-history`
- Relaciones directas: recursos anidados → `/api/users/{id}/pets`
- Auth: `Authorization: Bearer <access_token>` en cada request protegido
- Errores: `{ "error": "mensaje", "code": "SNAKE_CASE_CODE" }`

## Autenticación

JWT con refresh token. `Microsoft.AspNetCore.Authentication.JwtBearer` + ASP.NET Core Identity.

```
POST /api/auth/login    → { access_token, refresh_token }
POST /api/auth/refresh  → { access_token, refresh_token }
POST /api/auth/logout   → revoca refresh_token
```

## Módulos (64 casos de uso)

| Módulo | Estado |
|--------|--------|
| Gestión de usuarios y acceso (UC-01–08) | Pendiente |
| Perfil del perro (UC-09–14) | In Progress |
| Walk Planner (UC-15–20) | Pendiente |
| Walker Hub (UC-21–27) | Pendiente |
| Matching y recomendaciones (UC-28–31) | Pendiente |
| Reserva y gestión del servicio (UC-32–38) | Pendiente |
| Seguimiento en tiempo real / GPS (UC-39–44) | Pendiente |
| Alertas perros perdidos/encontrados (UC-45–49) | Pendiente |
| AI Coach (UC-50–55) | Pendiente |
| Dashboard de actividad (UC-56–58) | Pendiente |
| Reputación y calificaciones (UC-59–61) | Pendiente |
| Administración (UC-62–64) | Pendiente |
