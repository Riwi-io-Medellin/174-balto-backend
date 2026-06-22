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
          "201": { "description": "Created" },
          "400": { "description": "Bad Request" },
          "409": { "description": "Conflict" }
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
          "200": { "description": "OK" },
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
          "200": { "description": "OK" },
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
          "404": { "description": "Not Found" }
        }
      }
    },
    "/api/users": {
      "get": {
        "tags": ["Users"],
        "summary": "List users",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK" }
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
          "201": { "description": "Created" },
          "400": { "description": "Bad Request" }
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
          "200": { "description": "OK" },
          "404": { "description": "Not Found" }
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
          "200": { "description": "OK" },
          "404": { "description": "Not Found" }
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
          "404": { "description": "Not Found" }
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
        "required": ["firstName", "lastName", "email", "password"],
        "properties": {
          "firstName": { "type": "string" },
          "lastName": { "type": "string" },
          "email": { "type": "string" },
          "password": { "type": "string" }
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
      "CreateUserRequest": {
        "type": "object",
        "required": ["fullName", "email", "role"],
        "properties": {
          "fullName": { "type": "string" },
          "email": { "type": "string" },
          "role": { "type": "string" },
          "isActive": { "type": "boolean", "default": true }
        }
      },
      "UpdateUserRequest": {
        "type": "object",
        "required": ["fullName", "email", "role", "isActive"],
        "properties": {
          "fullName": { "type": "string" },
          "email": { "type": "string" },
          "role": { "type": "string" },
          "isActive": { "type": "boolean" }
        }
      }
    }
  }
}
""";
}
