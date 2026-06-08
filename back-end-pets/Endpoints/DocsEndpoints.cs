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
    "description": "Pets CRUD for the Balto backend."
  },
  "security": [
    {
      "BearerAuth": []
    }
  ],
  "paths": {
    "/api/users/{userId}/pets": {
      "get": {
        "tags": ["Pets"],
        "summary": "List pets for a user",
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "responses": {
          "200": {
            "description": "OK"
          },
          "401": {
            "description": "Unauthorized"
          }
        }
      },
      "post": {
        "tags": ["Pets"],
        "summary": "Create a pet for a user",
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/CreatePetRequest" }
            }
          }
        },
        "responses": {
          "201": {
            "description": "Created"
          },
          "400": {
            "description": "Bad Request"
          },
          "401": {
            "description": "Unauthorized"
          }
        }
      }
    },
    "/api/users/{userId}/pets/{id}": {
      "get": {
        "tags": ["Pets"],
        "summary": "Get a pet by id",
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          },
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "responses": {
          "200": { "description": "OK" },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Not Found" }
        }
      },
      "put": {
        "tags": ["Pets"],
        "summary": "Update a pet",
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          },
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
              "schema": { "$ref": "#/components/schemas/UpdatePetRequest" }
            }
          }
        },
        "responses": {
          "200": { "description": "OK" },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Not Found" }
        }
      },
      "delete": {
        "tags": ["Pets"],
        "summary": "Delete a pet",
        "parameters": [
          {
            "name": "userId",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          },
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": { "type": "string", "format": "uuid" }
          }
        ],
        "responses": {
          "204": { "description": "No Content" },
          "401": { "description": "Unauthorized" },
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
      "CreatePetRequest": {
        "type": "object",
        "required": ["name", "species"],
        "properties": {
          "name": { "type": "string" },
          "species": { "type": "string" },
          "breed": { "type": ["string", "null"] },
          "birthDate": { "type": ["string", "null"], "format": "date-time" },
          "description": { "type": ["string", "null"] },
          "photoUrl": { "type": ["string", "null"] }
        }
      },
      "UpdatePetRequest": {
        "type": "object",
        "required": ["name", "species"],
        "properties": {
          "name": { "type": "string" },
          "species": { "type": "string" },
          "breed": { "type": ["string", "null"] },
          "birthDate": { "type": ["string", "null"], "format": "date-time" },
          "description": { "type": ["string", "null"] },
          "photoUrl": { "type": ["string", "null"] }
        }
      }
    }
  }
}
""";
}
