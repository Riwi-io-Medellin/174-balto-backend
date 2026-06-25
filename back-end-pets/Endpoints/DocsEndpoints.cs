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
    "description": "Backend API for Balto — pet walking and veterinary services platform."
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
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/RegisterRequest" } } }
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
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/LoginRequest" } } }
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
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/RefreshTokenRequest" } } }
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
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/LogoutRequest" } } }
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
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreateUserRequest" } } }
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
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "put": {
        "tags": ["Users"],
        "summary": "Update a user",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UpdateUserRequest" } } }
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
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
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
      "get": {
        "tags": ["Walkers"],
        "summary": "List all walkers",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "available", "in": "query", "required": false, "schema": { "type": "boolean" } },
          { "name": "workLocation", "in": "query", "required": false, "schema": { "type": "string" } }
        ],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkerResponse" } } } } }
        }
      },
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
    "/api/walkers/{id}": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get a walker by id",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/recommendations": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get recommended walkers ordered by availability, location and experience",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "workLocation", "in": "query", "required": false, "schema": { "type": "string" } }
        ],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkerRecommendationResponse" } } } } }
        }
      }
    },
    "/api/walkers/me/gallery": {
      "post": {
        "tags": ["Walkers"],
        "summary": "Add a photo to the current walker's gallery",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AddWalkerPhotoRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerGalleryResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/{walkerId}/gallery": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get all photos for a walker",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "walkerId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkerGalleryResponse" } } } } }
        }
      }
    },
    "/api/walkers/me/gallery/{photoId}": {
      "delete": {
        "tags": ["Walkers"],
        "summary": "Delete a photo from the current walker's gallery",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "photoId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "204": { "description": "No Content" },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/me/documents": {
      "post": {
        "tags": ["Walkers"],
        "summary": "Add a document to the current walker's profile",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AddWalkerDocumentRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerDocumentResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/{walkerId}/documents": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get all documents for a walker",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "walkerId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkerDocumentResponse" } } } } }
        }
      }
    },
    "/api/walkers/me/documents/{documentId}": {
      "delete": {
        "tags": ["Walkers"],
        "summary": "Delete a document from the current walker's profile",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "documentId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "204": { "description": "No Content" },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/businesses": {
      "get": {
        "tags": ["Businesses"],
        "summary": "List all businesses",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "type", "in": "query", "required": false, "schema": { "type": "string", "enum": ["veterinary", "grooming", "shelter", "petshop", "other"] } },
          { "name": "location", "in": "query", "required": false, "schema": { "type": "string" } }
        ],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/BusinessResponse" } } } } }
        }
      },
      "post": {
        "tags": ["Businesses"],
        "summary": "Register a new business for the current user",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreateBusinessRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BusinessResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/businesses/{id}": {
      "get": {
        "tags": ["Businesses"],
        "summary": "Get a business by id",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BusinessResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/businesses/{businessId}/documents": {
      "post": {
        "tags": ["Businesses"],
        "summary": "Add a document to a business (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AddBusinessDocumentRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BusinessDocumentResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "get": {
        "tags": ["Businesses"],
        "summary": "Get all documents for a business",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/BusinessDocumentResponse" } } } } }
        }
      }
    },
    "/api/businesses/{businessId}/documents/{documentId}": {
      "delete": {
        "tags": ["Businesses"],
        "summary": "Delete a document from a business (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } },
          { "name": "documentId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }
        ],
        "responses": {
          "204": { "description": "No Content" },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/businesses/{businessId}/services": {
      "post": {
        "tags": ["Businesses"],
        "summary": "Add a service to a business (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreateBusinessServiceRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BusinessServiceResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "get": {
        "tags": ["Businesses"],
        "summary": "Get all services for a business",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/BusinessServiceResponse" } } } } }
        }
      }
    },
    "/api/businesses/{businessId}/services/{serviceId}": {
      "put": {
        "tags": ["Businesses"],
        "summary": "Update a service of a business (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } },
          { "name": "serviceId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }
        ],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UpdateBusinessServiceRequest" } } }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BusinessServiceResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "delete": {
        "tags": ["Businesses"],
        "summary": "Delete a service from a business (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } },
          { "name": "serviceId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }
        ],
        "responses": {
          "204": { "description": "No Content" },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/pets": {
      "post": {
        "tags": ["Pets"],
        "summary": "Register a new pet for the current user",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreatePetRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/PetResponse" } } } }
        }
      }
    },
    "/api/pets/me": {
      "get": {
        "tags": ["Pets"],
        "summary": "Get all pets of the current user",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/PetResponse" } } } } }
        }
      }
    },
    "/api/pets/{id}": {
      "get": {
        "tags": ["Pets"],
        "summary": "Get a pet by id",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/PetResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "put": {
        "tags": ["Pets"],
        "summary": "Update a pet owned by the current user",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UpdatePetRequest" } } }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/PetResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "delete": {
        "tags": ["Pets"],
        "summary": "Delete a pet owned by the current user",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "204": { "description": "No Content" },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/pets/{petId}/history": {
      "post": {
        "tags": ["Pets"],
        "summary": "Add a history entry to a pet (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "petId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreatePetHistoryRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/PetHistoryResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "get": {
        "tags": ["Pets"],
        "summary": "Get all history entries for a pet (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "petId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/PetHistoryResponse" } } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/pets/{petId}/history/{historyId}": {
      "delete": {
        "tags": ["Pets"],
        "summary": "Delete a history entry from a pet (owner only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "petId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } },
          { "name": "historyId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }
        ],
        "responses": {
          "204": { "description": "No Content" },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walking-history": {
      "post": {
        "tags": ["WalkingHistory"],
        "summary": "Create a walking history entry for a pet",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreateWalkingHistoryRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkingHistoryResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walking-history/me": {
      "get": {
        "tags": ["WalkingHistory"],
        "summary": "Get all walking history entries for the current user",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkingHistoryResponse" } } } } }
        }
      }
    },
    "/api/walking-history/walker": {
      "get": {
        "tags": ["WalkingHistory"],
        "summary": "Get all walking history entries assigned to the current walker",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkingHistoryResponse" } } } } }
        }
      }
    },
    "/api/walking-history/{id}": {
      "get": {
        "tags": ["WalkingHistory"],
        "summary": "Get a walking history entry by id",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkingHistoryResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/feedback/walkers": {
      "post": {
        "tags": ["Feedback"],
        "summary": "Leave feedback for a walker (requires walk history)",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreateWalkerFeedbackRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/FeedbackResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/feedback/businesses": {
      "post": {
        "tags": ["Feedback"],
        "summary": "Leave feedback for a business (any authenticated user)",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/CreateBusinessFeedbackRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/FeedbackResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/feedback/walkers/{walkerId}": {
      "get": {
        "tags": ["Feedback"],
        "summary": "Get all feedback and average rating for a walker",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "walkerId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/FeedbackSummaryResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/feedback/businesses/{businessId}": {
      "get": {
        "tags": ["Feedback"],
        "summary": "Get all feedback and average rating for a business",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "businessId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/FeedbackSummaryResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/storage/upload": {
      "post": {
        "tags": ["Storage"],
        "summary": "Upload a file to R2 storage and get back the public URL",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "folder", "in": "query", "required": true, "schema": { "type": "string", "enum": ["pets", "walkers", "businesses", "documents"] } }
        ],
        "requestBody": {
          "required": true,
          "content": { "multipart/form-data": { "schema": { "type": "object", "properties": { "file": { "type": "string", "format": "binary" } }, "required": ["file"] } } }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UploadResponse" } } } },
          "400": { "description": "Bad Request", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Start a new walk session (assigned walker only)",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/StartWalkSessionRequest" } } }
        },
        "responses": {
          "201": { "description": "Created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkSessionResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}/location": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Add a GPS point and broadcast to connected clients",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/AddLocationRequest" } } }
        },
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkRoutePointResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}/pause": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Pause an in-progress walk session",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkSessionResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}/resume": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Resume a paused walk session",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkSessionResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}/complete": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Complete a walk session",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkSessionResponse" } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Conflict", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}/route": {
      "get": {
        "tags": ["WalkSessions"],
        "summary": "Get all GPS route points for a session (walker or pet owner)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/WalkRoutePointResponse" } } } } },
          "403": { "description": "Forbidden", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    }
  },
  "components": {
    "securitySchemes": {
      "BearerAuth": { "type": "http", "scheme": "bearer", "bearerFormat": "JWT" }
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
        "properties": { "refreshToken": { "type": "string" } }
      },
      "LogoutRequest": {
        "type": "object",
        "required": ["refreshToken"],
        "properties": { "refreshToken": { "type": "string" } }
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
      "WalkerRecommendationResponse": {
        "type": "object",
        "required": ["id", "userId", "available", "verificationStatus", "reasons"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "available": { "type": "boolean" },
          "workLocation": { "type": "string", "nullable": true },
          "experience": { "type": "string", "nullable": true },
          "description": { "type": "string", "nullable": true },
          "verificationStatus": { "type": "string" },
          "reasons": { "type": "array", "items": { "type": "string" } }
        }
      },
      "AddWalkerPhotoRequest": {
        "type": "object",
        "required": ["photoUrl"],
        "properties": { "photoUrl": { "type": "string" } }
      },
      "WalkerGalleryResponse": {
        "type": "object",
        "required": ["id", "walkerId", "photoUrl", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "photoUrl": { "type": "string" },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "AddWalkerDocumentRequest": {
        "type": "object",
        "required": ["documentType", "fileUrl"],
        "properties": {
          "documentType": { "type": "string" },
          "fileUrl": { "type": "string" }
        }
      },
      "WalkerDocumentResponse": {
        "type": "object",
        "required": ["id", "walkerId", "documentType", "fileUrl", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "documentType": { "type": "string" },
          "fileUrl": { "type": "string" },
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
      },
      "AddBusinessDocumentRequest": {
        "type": "object",
        "required": ["documentType", "fileUrl"],
        "properties": {
          "documentType": { "type": "string" },
          "fileUrl": { "type": "string" }
        }
      },
      "BusinessDocumentResponse": {
        "type": "object",
        "required": ["id", "businessId", "documentType", "fileUrl", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "businessId": { "type": "string", "format": "uuid" },
          "documentType": { "type": "string" },
          "fileUrl": { "type": "string" },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "CreateBusinessServiceRequest": {
        "type": "object",
        "required": ["serviceType", "price"],
        "properties": {
          "serviceType": { "type": "string" },
          "price": { "type": "number", "format": "double" },
          "description": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true }
        }
      },
      "UpdateBusinessServiceRequest": {
        "type": "object",
        "required": ["serviceType", "price"],
        "properties": {
          "serviceType": { "type": "string" },
          "price": { "type": "number", "format": "double" },
          "description": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true }
        }
      },
      "BusinessServiceResponse": {
        "type": "object",
        "required": ["id", "businessId", "serviceType", "price", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "businessId": { "type": "string", "format": "uuid" },
          "serviceType": { "type": "string" },
          "description": { "type": "string", "nullable": true },
          "price": { "type": "number", "format": "double" },
          "photoUrl": { "type": "string", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "CreatePetRequest": {
        "type": "object",
        "required": ["name"],
        "properties": {
          "name": { "type": "string" },
          "species": { "type": "string", "nullable": true },
          "breed": { "type": "string", "nullable": true },
          "birthDate": { "type": "string", "format": "date-time", "nullable": true },
          "description": { "type": "string", "nullable": true },
          "weight": { "type": "number", "format": "double", "nullable": true, "minimum": 0, "maximum": 300 }
        }
      },
      "UpdatePetRequest": {
        "type": "object",
        "required": ["name"],
        "properties": {
          "name": { "type": "string" },
          "species": { "type": "string", "nullable": true },
          "breed": { "type": "string", "nullable": true },
          "birthDate": { "type": "string", "format": "date-time", "nullable": true },
          "description": { "type": "string", "nullable": true },
          "weight": { "type": "number", "format": "double", "nullable": true, "minimum": 0, "maximum": 300 }
        }
      },
      "PetResponse": {
        "type": "object",
        "required": ["id", "userId", "name", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "name": { "type": "string" },
          "species": { "type": "string", "nullable": true },
          "breed": { "type": "string", "nullable": true },
          "birthDate": { "type": "string", "format": "date-time", "nullable": true },
          "description": { "type": "string", "nullable": true },
          "photoUrl": { "type": "string", "nullable": true },
          "weight": { "type": "number", "format": "double", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "CreatePetHistoryRequest": {
        "type": "object",
        "required": ["title"],
        "properties": {
          "title": { "type": "string" },
          "description": { "type": "string", "nullable": true },
          "documentUrl": { "type": "string", "nullable": true }
        }
      },
      "PetHistoryResponse": {
        "type": "object",
        "required": ["id", "petId", "title", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "petId": { "type": "string", "format": "uuid" },
          "title": { "type": "string" },
          "description": { "type": "string", "nullable": true },
          "documentUrl": { "type": "string", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "CreateWalkingHistoryRequest": {
        "type": "object",
        "required": ["petId", "walkerId"],
        "properties": {
          "petId": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "startTime": { "type": "string", "format": "date-time", "nullable": true },
          "cost": { "type": "number", "format": "double", "nullable": true }
        }
      },
      "WalkingHistoryResponse": {
        "type": "object",
        "required": ["id", "userId", "petId", "walkerId", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "petId": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "cost": { "type": "number", "format": "double", "nullable": true },
          "startTime": { "type": "string", "format": "date-time", "nullable": true },
          "endTime": { "type": "string", "format": "date-time", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "CreateWalkerFeedbackRequest": {
        "type": "object",
        "required": ["walkerId", "rating"],
        "properties": {
          "walkerId": { "type": "string", "format": "uuid" },
          "rating": { "type": "integer", "minimum": 1, "maximum": 5 },
          "comment": { "type": "string", "nullable": true }
        }
      },
      "CreateBusinessFeedbackRequest": {
        "type": "object",
        "required": ["businessId", "rating"],
        "properties": {
          "businessId": { "type": "string", "format": "uuid" },
          "rating": { "type": "integer", "minimum": 1, "maximum": 5 },
          "comment": { "type": "string", "nullable": true }
        }
      },
      "FeedbackResponse": {
        "type": "object",
        "required": ["id", "userId", "targetId", "targetType", "rating", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "targetId": { "type": "string", "format": "uuid" },
          "targetType": { "type": "string", "enum": ["walker", "business"] },
          "rating": { "type": "integer", "minimum": 1, "maximum": 5 },
          "comment": { "type": "string", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      },
      "FeedbackSummaryResponse": {
        "type": "object",
        "required": ["targetId", "targetType", "averageRating", "totalReviews", "reviews"],
        "properties": {
          "targetId": { "type": "string", "format": "uuid" },
          "targetType": { "type": "string", "enum": ["walker", "business"] },
          "averageRating": { "type": "number", "format": "double" },
          "totalReviews": { "type": "integer" },
          "reviews": { "type": "array", "items": { "$ref": "#/components/schemas/FeedbackResponse" } }
        }
      },
      "UploadResponse": {
        "type": "object",
        "required": ["url"],
        "properties": { "url": { "type": "string" } }
      },
      "StartWalkSessionRequest": {
        "type": "object",
        "required": ["petWalkingHistoryId"],
        "properties": { "petWalkingHistoryId": { "type": "string", "format": "uuid" } }
      },
      "AddLocationRequest": {
        "type": "object",
        "required": ["latitude", "longitude"],
        "properties": {
          "latitude": { "type": "number", "format": "double" },
          "longitude": { "type": "number", "format": "double" }
        }
      },
      "WalkSessionResponse": {
        "type": "object",
        "required": ["id", "petWalkingHistoryId", "status"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "petWalkingHistoryId": { "type": "string", "format": "uuid" },
          "status": { "type": "string", "enum": ["in_progress", "paused", "completed"] },
          "startedAt": { "type": "string", "format": "date-time", "nullable": true },
          "endedAt": { "type": "string", "format": "date-time", "nullable": true }
        }
      },
      "WalkRoutePointResponse": {
        "type": "object",
        "required": ["id", "walkSessionId", "latitude", "longitude", "createdAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "walkSessionId": { "type": "string", "format": "uuid" },
          "latitude": { "type": "number", "format": "double" },
          "longitude": { "type": "number", "format": "double" },
          "createdAt": { "type": "string", "format": "date-time" }
        }
      }
    }
  }
}
""";
}
