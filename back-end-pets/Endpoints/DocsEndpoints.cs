namespace BackEndPets.API.Endpoints;

public static class DocsEndpoints
{
    public static IEndpointRouteBuilder MapSwaggerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/swagger", () => Results.Content(SwaggerHtml, "text/html"))
            .ExcludeFromDescription();

        app.MapGet("/swagger/v1/swagger.json", () => Results.Text(OpenApiJson, "application/json"))
            .ExcludeFromDescription();

        return app;
    }

    private const string SwaggerHtml = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Balto API Docs</title>
  <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css">
  <style>
    body { margin: 0; background: #f6f7fb; }
    .topbar { display: none; }
  </style>
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
  <script>
    window.ui = SwaggerUIBundle({
      url: '/swagger/v1/swagger.json',
      dom_id: '#swagger-ui',
      deepLinking: true,
      persistAuthorization: true,
      presets: [SwaggerUIBundle.presets.apis],
      layout: 'BaseLayout'
    });
  </script>
</body>
</html>
""";

    private const string OpenApiJson = """
{
  "openapi": "3.0.3",
  "info": {
    "title": "Balto API",
    "version": "v1",
    "description": "Identity auth and users CRUD for the Balto backend."
  },
  "paths": {
    "/health": {
      "get": {
        "tags": ["Health"],
        "summary": "Health check",
        "responses": {
          "200": { "description": "OK", "content": { "text/plain": { "schema": { "type": "string" } } } }
        }
      }
    },
    "/api/auth/register": {
      "post": {
        "tags": ["Auth"],
        "summary": "Create an account and get tokens",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/RegisterRequest" }
            }
          }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AuthResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/auth/login": {
      "post": {
        "tags": ["Auth"],
        "summary": "Log in and get tokens",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/LoginRequest" }
            }
          }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AuthResponse" } } } },
          "401": { "description": "Unauthorized" }
        }
      }
    },
    "/api/auth/refresh": {
      "post": {
        "tags": ["Auth"],
        "summary": "Refresh the access token",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/RefreshTokenRequest" }
            }
          }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AuthResponse" } } } },
          "401": { "description": "Unauthorized" }
        }
      }
    },
    "/api/auth/logout": {
      "post": {
        "tags": ["Auth"],
        "summary": "Revoke a refresh token",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/LogoutRequest" }
            }
          }
        },
        "responses": {
          "204": { "description": "No Content" },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/users": {
      "get": {
        "tags": ["Users"],
        "summary": "List users",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/UserResponse" } } } } }
        }
      },
      "post": {
        "tags": ["Users"],
        "summary": "Create a user",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/CreateUserRequest" }
            }
          }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/users/{id}": {
      "get": {
        "tags": ["Users"],
        "summary": "Get a user by id",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "put": {
        "tags": ["Users"],
        "summary": "Update a user",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/UpdateUserRequest" }
            }
          }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "delete": {
        "tags": ["Users"],
        "summary": "Delete a user",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "responses": {
          "204": { "description": "No Content" },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/me": {
      "get": {
        "tags": ["Me"],
        "summary": "Get the authenticated user's aggregated profile",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/MeResponse" } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers": {
      "post": {
        "tags": ["Walkers"],
        "summary": "Register the current user as a walker",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerResponse" } } } },
          "401": { "description": "Unauthorized" },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/businesses": {
      "post": {
        "tags": ["Businesses"],
        "summary": "Register a new business for the current user",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/CreateBusinessRequest" }
            }
          }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BusinessResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    }
  },
  "components": {
    "securitySchemes": {
      "BearerAuth": {
        "type": "http",
        "scheme": "bearer",
        "bearerFormat": "JWT"
      }
    },
    "schemas": {
      "RegisterRequest": {
        "type": "object",
        "required": ["firstName", "lastName", "email", "password", "idNumber", "idType", "phone"],
        "properties": {
          "firstName": { "type": "string" },
          "lastName": { "type": "string" },
          "email": { "type": "string", "format": "email" },
          "password": { "type": "string" },
          "idNumber": { "type": "string" },
          "idType": { "type": "string", "enum": ["CC", "CE", "Passport", "TI"] },
          "phone": { "type": "string" },
          "phoneExtra": { "type": "string", "nullable": true },
          "location": { "type": "string", "nullable": true },
          "address": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true }
        }
      },
      "LoginRequest": {
        "type": "object",
        "required": ["email", "password"],
        "properties": {
          "email": { "type": "string" },
          "password": { "type": "string" }
        }
      },
      "RefreshTokenRequest": {
        "type": "object",
        "required": ["refreshToken"],
        "properties": {
          "refreshToken": { "type": "string" }
        }
      },
      "LogoutRequest": {
        "type": "object",
        "required": ["refreshToken"],
        "properties": {
          "refreshToken": { "type": "string" }
        }
      },
      "AuthResponse": {
        "type": "object",
        "required": ["accessToken", "refreshToken", "expiresAt"],
        "properties": {
          "accessToken": { "type": "string" },
          "refreshToken": { "type": "string" },
          "expiresAt": { "type": "string", "format": "date-time" }
        }
      },
      "ApiErrorResponse": {
        "type": "object",
        "required": ["error", "code"],
        "properties": {
          "error": { "type": "string" },
          "code": { "type": "string" }
        }
      },
      "CreateUserRequest": {
        "type": "object",
        "required": ["firstName", "lastName", "email", "password", "idNumber", "idType", "phone"],
        "properties": {
          "firstName": { "type": "string" },
          "lastName": { "type": "string" },
          "email": { "type": "string" },
          "password": { "type": "string" },
          "idNumber": { "type": "string" },
          "idType": { "type": "string", "enum": ["CC", "CE", "Passport", "TI"] },
          "phone": { "type": "string" },
          "phoneExtra": { "type": "string", "nullable": true },
          "location": { "type": "string", "nullable": true },
          "address": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true }
        }
      },
      "UpdateUserRequest": {
        "type": "object",
        "required": ["firstName", "lastName", "idNumber", "idType", "phone"],
        "properties": {
          "firstName": { "type": "string" },
          "lastName": { "type": "string" },
          "idNumber": { "type": "string" },
          "idType": { "type": "string", "enum": ["CC", "CE", "Passport", "TI"] },
          "phone": { "type": "string" },
          "phoneExtra": { "type": "string", "nullable": true },
          "location": { "type": "string", "nullable": true },
          "address": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true }
        }
      },
      "UserResponse": {
        "type": "object",
        "required": ["id", "firstName", "lastName", "email", "idNumber", "idType", "phone", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "firstName": { "type": "string" },
          "lastName": { "type": "string" },
          "email": { "type": "string" },
          "idNumber": { "type": "string" },
          "idType": { "type": "string", "enum": ["CC", "CE", "Passport", "TI"] },
          "phone": { "type": "string" },
          "phoneExtra": { "type": "string", "nullable": true },
          "location": { "type": "string", "nullable": true },
          "address": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "MeResponse": {
        "type": "object",
        "required": ["id", "firstName", "lastName", "email", "isWalker", "businesses"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "firstName": { "type": "string" },
          "lastName": { "type": "string" },
          "email": { "type": "string" },
          "isWalker": { "type": "boolean" },
          "walkerStatus": { "type": "string", "nullable": true },
          "businesses": { "type": "array", "items": { "$ref": "#/components/schemas/BusinessSummary" } }
        }
      },
      "BusinessSummary": {
        "type": "object",
        "required": ["id", "name", "verificationStatus"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "type": { "type": "string", "nullable": true, "enum": ["veterinary", "grooming", "shelter", "petshop", "other"] },
          "verificationStatus": { "type": "string", "enum": ["pending", "approved", "rejected", "suspended"] }
        }
      },
      "WalkerResponse": {
        "type": "object",
        "required": ["id", "userId", "verificationStatus", "available", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "verificationStatus": { "type": "string", "enum": ["pending", "approved", "rejected", "suspended"] },
          "available": { "type": "boolean" },
          "workLocation": { "type": "string", "nullable": true },
          "experience": { "type": "string", "nullable": true },
          "description": { "type": "string", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "CreateBusinessRequest": {
        "type": "object",
        "required": ["name", "nit", "email", "phone"],
        "properties": {
          "name": { "type": "string" },
          "nit": { "type": "string" },
          "email": { "type": "string", "format": "email" },
          "phone": { "type": "integer", "format": "int64" },
          "type": { "type": "string", "nullable": true, "enum": ["veterinary", "grooming", "shelter", "petshop", "other"] },
          "location": { "type": "string", "nullable": true },
          "address": { "type": "string", "nullable": true }
        }
      },
      "BusinessResponse": {
        "type": "object",
        "required": ["id", "ownerUserId", "name", "nit", "email", "phone", "verificationStatus", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "ownerUserId": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "nit": { "type": "string" },
          "email": { "type": "string" },
          "phone": { "type": "integer", "format": "int64" },
          "type": { "type": "string", "nullable": true, "enum": ["veterinary", "grooming", "shelter", "petshop", "other"] },
          "location": { "type": "string", "nullable": true },
          "address": { "type": "string", "nullable": true },
          "verificationStatus": { "type": "string", "enum": ["pending", "approved", "rejected", "suspended"] },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      }
    }
  }
}
""";
}
