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
    "description": "CRUD endpoints for the users and pets tables."
  },
  "paths": {
    "/api/users": {
      "get": {
        "tags": ["Users"],
        "summary": "List users",
        "responses": {
          "200": { "description": "OK" }
        }
      },
      "post": {
        "tags": ["Users"],
        "summary": "Create a user",
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
    },
    "/api/pets": {
      "get": {
        "tags": ["Pets"],
        "summary": "List pets",
        "responses": {
          "200": { "description": "OK" }
        }
      },
      "post": {
        "tags": ["Pets"],
        "summary": "Create a pet",
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/CreatePetRequest" }
            }
          }
        },
        "responses": {
          "201": { "description": "Created" },
          "400": { "description": "Bad Request" }
        }
      }
    },
    "/api/pets/{id}": {
      "get": {
        "tags": ["Pets"],
        "summary": "Get a pet by id",
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
        "tags": ["Pets"],
        "summary": "Update a pet",
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
              "schema": { "$ref": "#/components/schemas/UpdatePetRequest" }
            }
          }
        },
        "responses": {
          "200": { "description": "OK" },
          "404": { "description": "Not Found" }
        }
      },
      "delete": {
        "tags": ["Pets"],
        "summary": "Delete a pet",
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
    "schemas": {
      "CreateUserRequest": {
        "type": "object",
        "required": ["first_name", "last_name", "email", "id_number", "id_type", "phone"],
        "properties": {
          "first_name": { "type": "string" },
          "last_name": { "type": "string" },
          "email": { "type": "string", "format": "email" },
          "id_number": { "type": "string" },
          "id_type": { "type": "string", "enum": ["CC", "CE", "Passport", "TI"] },
          "location": { "type": ["string", "null"] },
          "address": { "type": ["string", "null"] },
          "phone": { "type": "integer", "format": "int64" },
          "phone_extra": { "type": ["integer", "null"], "format": "int64" },
          "photo_url": { "type": ["string", "null"] }
        }
      },
      "UpdateUserRequest": {
        "type": "object",
        "required": ["first_name", "last_name", "email", "id_number", "id_type", "phone"],
        "properties": {
          "first_name": { "type": "string" },
          "last_name": { "type": "string" },
          "email": { "type": "string", "format": "email" },
          "id_number": { "type": "string" },
          "id_type": { "type": "string", "enum": ["CC", "CE", "Passport", "TI"] },
          "location": { "type": ["string", "null"] },
          "address": { "type": ["string", "null"] },
          "phone": { "type": "integer", "format": "int64" },
          "phone_extra": { "type": ["integer", "null"], "format": "int64" },
          "photo_url": { "type": ["string", "null"] }
        }
      },
      "UserResponse": {
        "type": "object",
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "first_name": { "type": "string" },
          "last_name": { "type": "string" },
          "email": { "type": "string", "format": "email" },
          "id_number": { "type": "string" },
          "id_type": { "type": "string" },
          "location": { "type": ["string", "null"] },
          "address": { "type": ["string", "null"] },
          "phone": { "type": "integer", "format": "int64" },
          "phone_extra": { "type": ["integer", "null"], "format": "int64" },
          "photo_url": { "type": ["string", "null"] },
          "created_at": { "type": "string", "format": "date-time" }
        }
      },
      "CreatePetRequest": {
        "type": "object",
        "required": ["user_id", "name", "species"],
        "properties": {
          "user_id": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "species": { "type": "string" },
          "breed": { "type": ["string", "null"] },
          "birth_date": { "type": ["string", "null"], "format": "date" },
          "description": { "type": ["string", "null"] },
          "photo_url": { "type": ["string", "null"] }
        }
      },
      "UpdatePetRequest": {
        "type": "object",
        "required": ["user_id", "name", "species"],
        "properties": {
          "user_id": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "species": { "type": "string" },
          "breed": { "type": ["string", "null"] },
          "birth_date": { "type": ["string", "null"], "format": "date" },
          "description": { "type": ["string", "null"] },
          "photo_url": { "type": ["string", "null"] }
        }
      },
      "PetResponse": {
        "type": "object",
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "user_id": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "species": { "type": "string" },
          "breed": { "type": ["string", "null"] },
          "birth_date": { "type": ["string", "null"], "format": "date" },
          "description": { "type": ["string", "null"] },
          "photo_url": { "type": ["string", "null"] },
          "created_at": { "type": "string", "format": "date-time" }
        }
      }
    }
  }
}
""";
}
