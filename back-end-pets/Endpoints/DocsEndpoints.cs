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
    "/api/walkers/search": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Search approved walkers by location, date and duration",
        "description": "Returns paginated approved walkers accepting bookings, filtered by geolocation radius and availability on the requested date. Walkers without stored coordinates are excluded. Sorted by distance ascending.",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "latitude",        "in": "query", "required": true,  "schema": { "type": "number", "format": "double", "example": 4.7110 } },
          { "name": "longitude",       "in": "query", "required": true,  "schema": { "type": "number", "format": "double", "example": -74.0721 } },
          { "name": "radiusKm",        "in": "query", "required": true,  "schema": { "type": "number", "format": "double", "example": 5.0 } },
          { "name": "date",            "in": "query", "required": true,  "schema": { "type": "string", "format": "date", "example": "2026-07-15" } },
          { "name": "durationMinutes", "in": "query", "required": true,  "schema": { "type": "integer", "enum": [30, 60, 90] } },
          { "name": "page",            "in": "query", "required": false, "schema": { "type": "integer", "default": 1, "minimum": 1 } },
          { "name": "pageSize",        "in": "query", "required": false, "schema": { "type": "integer", "default": 20, "minimum": 1, "maximum": 100 } }
        ],
        "responses": {
          "200": { "description": "Paginated walker results", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/PagedWalkerSummary" } } } },
          "400": { "description": "Invalid durationMinutes", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" }
        }
      }
    },
    "/api/walkers/{walkerId}": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get full walker profile with availability and rating",
        "description": "Returns complete profile, weekly availability, optional available slots for a date, rating summary and completed walk count.",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "walkerId",        "in": "path",  "required": true,  "schema": { "type": "string", "format": "uuid" } },
          { "name": "date",            "in": "query", "required": false, "schema": { "type": "string", "format": "date", "example": "2026-07-15" }, "description": "If provided with durationMinutes, computes available slots" },
          { "name": "durationMinutes", "in": "query", "required": false, "schema": { "type": "integer", "enum": [30, 60, 90] } }
        ],
        "responses": {
          "200": { "description": "Walker detail", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerDetailResponse" } } } },
          "401": { "description": "Unauthorized" },
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
    "/api/walkers/apply": {
      "post": {
        "tags": ["Walkers"],
        "summary": "Submit identity document and profile info to apply as a verified walker",
        "description": "Uploads the identity document to Cloudinary, runs OCR via GPT-4o-mini to extract name and document number, then approves or rejects the walker by comparing the extracted name against the authenticated user's name (word-overlap ≥ 50%).",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": {
            "multipart/form-data": {
              "schema": {
                "type": "object",
                "required": ["document", "workLocation", "experience"],
                "properties": {
                  "document": { "type": "string", "format": "binary", "description": "Identity document image (jpg, jpeg, png, webp — max 10 MB)" },
                  "workLocation": { "type": "string", "description": "Area or city where the walker operates" },
                  "experience": { "type": "string", "description": "Walker's experience description" },
                  "description": { "type": "string", "description": "Optional additional bio" }
                }
              }
            }
          }
        },
        "responses": {
          "200": { "description": "Application submitted — identity document uploaded, walker status set to under_review", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerApplyResponse" } } } },
          "400": { "description": "Bad Request — missing file, invalid type/size, or missing required fields", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "503": { "description": "Cloudinary upload service not configured", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/me": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get the authenticated user's walker profile",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerProfileResponse" } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "put": {
        "tags": ["Walkers"],
        "summary": "Update the authenticated user's walker profile (approved walkers only)",
        "description": "Partial update — only provided fields are changed. VerificationStatus, DocumentName, DocumentNumber and UserId cannot be modified.",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UpdateWalkerProfileRequest" } } }
        },
        "responses": {
          "200": { "description": "Profile updated", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/WalkerProfileResponse" } } } },
          "400": { "description": "Validation error", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Walker not approved yet", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
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
    "/api/walkers/me/availability": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get the authenticated walker's weekly availability",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilitySlotResponse" } } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "put": {
        "tags": ["Walkers"],
        "summary": "Replace the authenticated walker's full weekly availability (approved walkers only)",
        "description": "Replaces the entire weekly schedule atomically. Send an empty array to clear availability.",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilitySlotRequest" } },
              "example": [
                { "dayOfWeek": 1, "startTime": "08:00:00", "endTime": "12:00:00" },
                { "dayOfWeek": 1, "startTime": "14:00:00", "endTime": "18:00:00" },
                { "dayOfWeek": 3, "startTime": "09:00:00", "endTime": "17:00:00" }
              ]
            }
          }
        },
        "responses": {
          "200": { "description": "Availability replaced", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilitySlotResponse" } } } } },
          "400": { "description": "Validation error", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Walker not approved yet", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/me/availability/exceptions": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get the authenticated walker's availability exceptions",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilityExceptionResponse" } } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      },
      "put": {
        "tags": ["Walkers"],
        "summary": "Replace the authenticated walker's full list of availability exceptions (approved walkers only)",
        "description": "Replaces all date exceptions atomically. Dates with isUnavailable=true override the weekly schedule entirely; dates with isUnavailable=false replace the time window for that day.",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilityExceptionRequest" } },
              "example": [
                { "date": "2025-12-25", "isUnavailable": true, "startTime": null, "endTime": null },
                { "date": "2025-12-26", "isUnavailable": false, "startTime": "10:00:00", "endTime": "14:00:00" }
              ]
            }
          }
        },
        "responses": {
          "200": { "description": "Exceptions replaced", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilityExceptionResponse" } } } } },
          "400": { "description": "Validation error", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Walker not approved yet", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
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
    },
    "/api/walk-sessions/me/active": {
      "get": {
        "tags": ["WalkSessions"],
        "summary": "Get the authenticated walker's current active session",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "Active session", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingSessionResponse" }, "example": { "id": "a1b2c3d4-0000-0000-0000-000000000001", "walkerId": "a1b2c3d4-0000-0000-0000-000000000002", "bookingId": "a1b2c3d4-0000-0000-0000-000000000003", "status": "in_progress", "startedAt": "2026-07-15T09:00:00Z", "finishedAt": null, "totalDistanceMeters": null, "totalDurationSeconds": null } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker not found or no active session", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/start": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Start a walk session from an accepted booking (walker only)",
        "description": "Creates a WalkSession and transitions the booking from accepted → in_progress atomically. Fails if the walker already has another in-progress session.",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/StartSessionFromBookingRequest" }, "example": { "bookingId": "a1b2c3d4-0000-0000-0000-000000000003" } } }
        },
        "responses": {
          "201": { "description": "Session started", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingSessionResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "No walker profile or booking not assigned to you", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Booking not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Booking not accepted or walker already has an active session", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}": {
      "get": {
        "tags": ["WalkSessions"],
        "summary": "Get full session details by ID",
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "Session details", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingSessionResponse" } } } },
          "404": { "description": "Session not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-sessions/{sessionId}/finish": {
      "post": {
        "tags": ["WalkSessions"],
        "summary": "Finish an active walk session and complete the linked booking (walker only)",
        "description": "Sets session status to completed, records distance and duration, and transitions the booking from in_progress → completed atomically.",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "sessionId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "requestBody": {
          "required": true,
          "content": { "application/json": { "schema": { "$ref": "#/components/schemas/FinishSessionRequest" }, "example": { "totalDistanceMeters": 3200.5, "totalDurationSeconds": 2700 } } }
        },
        "responses": {
          "200": { "description": "Session finished", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingSessionResponse" }, "example": { "id": "a1b2c3d4-0000-0000-0000-000000000001", "walkerId": "a1b2c3d4-0000-0000-0000-000000000002", "bookingId": "a1b2c3d4-0000-0000-0000-000000000003", "status": "completed", "startedAt": "2026-07-15T09:00:00Z", "finishedAt": "2026-07-15T09:45:00Z", "totalDistanceMeters": 3200.5, "totalDurationSeconds": 2700 } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "No walker profile or session is not yours", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Session or linked booking not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Session not active or not booking-based", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-bookings": {
      "post": {
        "tags": ["WalkBookings"],
        "summary": "Create a new walk booking",
        "description": "The walker must be approved and accepting bookings. slotStart must match one of the walker's available slots (use GET /walkers/{walkerId}/available-slots first). Walker's hourly rate is snapshotted and totalPrice is calculated at creation time.",
        "security": [{ "BearerAuth": [] }],
        "requestBody": {
          "required": true,
          "content": {
            "application/json": {
              "schema": { "$ref": "#/components/schemas/CreateBookingRequest" },
              "example": {
                "walkerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "petId": "7b3e1c82-1234-4abc-9def-000000000001",
                "slotStart": "2026-07-15T09:00:00Z",
                "durationMinutes": 60,
                "specialInstructions": "Please bring water for the dog and avoid the park on the left."
              }
            }
          }
        },
        "responses": {
          "201": { "description": "Booking created", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingResponse" } } } },
          "400": { "description": "Invalid duration or slot in the past", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "Pet not owned by current user", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Pet or walker not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Walker not approved, not accepting bookings, or requested slot is unavailable", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-bookings/me": {
      "get": {
        "tags": ["WalkBookings"],
        "summary": "Get the authenticated user's bookings",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "status", "in": "query", "required": false, "schema": { "type": "string", "enum": ["pending", "accepted", "rejected", "walker_cancelled", "owner_cancelled", "completed"] }, "description": "Filter by booking status" }
        ],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/BookingResponse" } } } } },
          "401": { "description": "Unauthorized" }
        }
      }
    },
    "/api/walk-bookings/{id}": {
      "get": {
        "tags": ["WalkBookings"],
        "summary": "Get booking details (owner or assigned walker only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "Forbidden — not the client nor the assigned walker", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Not Found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-bookings/{id}/accept": {
      "post": {
        "tags": ["WalkBookings"],
        "summary": "Accept a pending booking (walker only)",
        "description": "Transactionally updates the booking to accepted and creates a WalkSession set to start at the requested date.",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "Booking accepted", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "Booking not assigned to this walker", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Walker profile or booking not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Booking is not pending", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-bookings/{id}/reject": {
      "post": {
        "tags": ["WalkBookings"],
        "summary": "Reject a booking (walker only)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "Booking rejected", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "Booking not assigned to this walker", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Walker profile or booking not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Booking already resolved or completed", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-bookings/{id}/cancel": {
      "post": {
        "tags": ["WalkBookings"],
        "summary": "Cancel a booking as the assigned walker (sets status to walker_cancelled)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "Booking cancelled by walker", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "Booking not assigned to this walker", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Walker profile or booking not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Booking is already in a terminal state", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walk-bookings/{id}/owner-cancel": {
      "post": {
        "tags": ["WalkBookings"],
        "summary": "Cancel a booking as the booking owner (sets status to owner_cancelled)",
        "security": [{ "BearerAuth": [] }],
        "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } }],
        "responses": {
          "200": { "description": "Booking cancelled by owner", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/BookingResponse" } } } },
          "401": { "description": "Unauthorized" },
          "403": { "description": "You do not own this booking", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "404": { "description": "Booking not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Booking is already in a terminal state", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/me/bookings": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get all bookings for the authenticated walker with optional status filter",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "status", "in": "query", "required": false, "schema": { "type": "string", "enum": ["pending", "accepted", "rejected", "walker_cancelled", "owner_cancelled", "completed"] }, "description": "Filter by booking status" }
        ],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/BookingResponse" } } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/me/pending-bookings": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get pending booking requests for the authenticated walker",
        "security": [{ "BearerAuth": [] }],
        "responses": {
          "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/BookingResponse" } } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker profile not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
        }
      }
    },
    "/api/walkers/{walkerId}/available-slots": {
      "get": {
        "tags": ["Walkers"],
        "summary": "Get available booking slots for a walker on a given date",
        "description": "Generates time slots every 30 minutes that fit within the walker's availability for the requested date. Exceptions override the weekly schedule. Walker must be approved and accepting bookings.",
        "security": [{ "BearerAuth": [] }],
        "parameters": [
          { "name": "walkerId", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" }, "description": "Walker entity id" },
          { "name": "date", "in": "query", "required": true, "schema": { "type": "string", "format": "date", "example": "2026-07-15" }, "description": "Date to query (ISO 8601 date)" },
          { "name": "durationMinutes", "in": "query", "required": true, "schema": { "type": "integer", "enum": [30, 60, 90] }, "description": "Walk duration in minutes — must be 30, 60, or 90" }
        ],
        "responses": {
          "200": {
            "description": "Available slots",
            "content": {
              "application/json": {
                "schema": { "type": "array", "items": { "$ref": "#/components/schemas/AvailableSlotResponse" } },
                "example": [
                  { "start": "2026-07-15T08:00:00Z", "end": "2026-07-15T09:00:00Z" },
                  { "start": "2026-07-15T08:30:00Z", "end": "2026-07-15T09:30:00Z" },
                  { "start": "2026-07-15T09:00:00Z", "end": "2026-07-15T10:00:00Z" }
                ]
              }
            }
          },
          "400": { "description": "Invalid duration (must be 30, 60, or 90)", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "401": { "description": "Unauthorized" },
          "404": { "description": "Walker not found", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } },
          "409": { "description": "Walker not approved or not accepting bookings", "content": { "application/json": { "schema": { "$ref": "#/components/schemas/ApiErrorResponse" } } } }
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
      "WalkerApplyResponse": {
        "type": "object",
        "required": ["walkerId", "userId", "verificationStatus", "documentUrl", "message"],
        "properties": {
          "walkerId": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "verificationStatus": { "type": "string", "enum": ["pending", "approved", "rejected", "suspended"], "description": "approved when extracted name matches account name (≥50% word overlap); rejected otherwise" },
          "documentUrl": { "type": "string", "description": "Cloudinary secure URL of the uploaded identity document" },
          "documentName": { "type": "string", "nullable": true, "description": "Full name as extracted from the document by GPT-4o-mini" },
          "documentNumber": { "type": "string", "nullable": true, "description": "Document number as extracted from the document by GPT-4o-mini" },
          "message": { "type": "string" }
        }
      },
      "WalkerProfileResponse": {
        "type": "object",
        "required": ["id", "userId", "verificationStatus", "available", "isAcceptingBookings", "createdAt", "updatedAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "verificationStatus": { "type": "string", "enum": ["pending", "approved", "rejected", "suspended"] },
          "available": { "type": "boolean" },
          "workLocation": { "type": "string", "nullable": true },
          "experience": { "type": "string", "nullable": true },
          "description": { "type": "string", "nullable": true },
          "bio": { "type": "string", "nullable": true },
          "hourlyRate": { "type": "number", "format": "double", "nullable": true },
          "serviceRadiusKm": { "type": "number", "format": "double", "nullable": true },
          "yearsOfExperience": { "type": "integer", "nullable": true },
          "isAcceptingBookings": { "type": "boolean" },
          "documentName": { "type": "string", "nullable": true },
          "documentNumber": { "type": "string", "nullable": true },
          "workLatitude": { "type": "number", "format": "double", "nullable": true },
          "workLongitude": { "type": "number", "format": "double", "nullable": true },
          "createdAt": { "type": "string", "format": "date-time" },
          "updatedAt": { "type": "string", "format": "date-time" }
        }
      },
      "UpdateWalkerProfileRequest": {
        "type": "object",
        "properties": {
          "bio": { "type": "string", "nullable": true, "description": "Walker biography shown on their public profile" },
          "hourlyRate": { "type": "number", "format": "double", "nullable": true, "minimum": 0, "description": "Rate charged per hour (>= 0)" },
          "serviceRadiusKm": { "type": "number", "format": "double", "nullable": true, "exclusiveMinimum": 0, "description": "Maximum km from work location the walker will travel (> 0)" },
          "yearsOfExperience": { "type": "integer", "nullable": true, "minimum": 0, "description": "Years of professional experience (>= 0)" },
          "isAcceptingBookings": { "type": "boolean", "nullable": true, "description": "Whether the walker is currently open to new bookings" },
          "workLatitude": { "type": "number", "format": "double", "nullable": true, "description": "Walker's work location latitude (used for marketplace search)" },
          "workLongitude": { "type": "number", "format": "double", "nullable": true, "description": "Walker's work location longitude (used for marketplace search)" }
        }
      },
      "AvailabilitySlotRequest": {
        "type": "object",
        "required": ["dayOfWeek", "startTime", "endTime"],
        "properties": {
          "dayOfWeek": { "type": "integer", "minimum": 0, "maximum": 6, "description": "0 = Sunday, 1 = Monday … 6 = Saturday" },
          "startTime": { "type": "string", "format": "time", "example": "08:00:00", "description": "ISO 8601 time — must be earlier than endTime" },
          "endTime": { "type": "string", "format": "time", "example": "12:00:00" }
        }
      },
      "AvailabilitySlotResponse": {
        "type": "object",
        "required": ["id", "walkerId", "dayOfWeek", "startTime", "endTime"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "dayOfWeek": { "type": "integer", "minimum": 0, "maximum": 6 },
          "startTime": { "type": "string", "format": "time" },
          "endTime": { "type": "string", "format": "time" }
        }
      },
      "AvailabilityExceptionRequest": {
        "type": "object",
        "required": ["date", "isUnavailable"],
        "properties": {
          "date": { "type": "string", "format": "date", "example": "2025-12-25", "description": "Must be unique in the request array" },
          "isUnavailable": { "type": "boolean", "description": "true = unavailable all day (startTime/endTime must be null); false = alternate hours (startTime/endTime required)" },
          "startTime": { "type": "string", "format": "time", "nullable": true, "example": "10:00:00" },
          "endTime": { "type": "string", "format": "time", "nullable": true, "example": "14:00:00" }
        }
      },
      "AvailabilityExceptionResponse": {
        "type": "object",
        "required": ["id", "walkerId", "date", "isUnavailable"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "date": { "type": "string", "format": "date" },
          "isUnavailable": { "type": "boolean" },
          "startTime": { "type": "string", "format": "time", "nullable": true },
          "endTime": { "type": "string", "format": "time", "nullable": true }
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
      },
      "StartSessionFromBookingRequest": {
        "type": "object",
        "required": ["bookingId"],
        "properties": {
          "bookingId": { "type": "string", "format": "uuid", "description": "ID of an accepted booking to start a session for" }
        }
      },
      "FinishSessionRequest": {
        "type": "object",
        "required": ["totalDistanceMeters", "totalDurationSeconds"],
        "properties": {
          "totalDistanceMeters": { "type": "number", "format": "double", "description": "Total GPS distance in meters", "example": 3200.5 },
          "totalDurationSeconds": { "type": "integer", "description": "Total elapsed duration in seconds", "example": 2700 }
        }
      },
      "BookingSessionResponse": {
        "type": "object",
        "required": ["id", "walkerId", "bookingId", "status", "startedAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid" },
          "bookingId": { "type": "string", "format": "uuid" },
          "status": { "type": "string", "enum": ["in_progress", "completed"] },
          "startedAt": { "type": "string", "format": "date-time" },
          "finishedAt": { "type": "string", "format": "date-time", "nullable": true },
          "totalDistanceMeters": { "type": "number", "format": "double", "nullable": true },
          "totalDurationSeconds": { "type": "integer", "nullable": true }
        }
      },
      "CreateBookingRequest": {
        "type": "object",
        "required": ["walkerId", "petId", "slotStart", "durationMinutes"],
        "properties": {
          "walkerId": { "type": "string", "format": "uuid", "description": "Walker entity id (not user id)" },
          "petId": { "type": "string", "format": "uuid" },
          "slotStart": { "type": "string", "format": "date-time", "description": "Walk start time (UTC). Must match one of the walker's available slots returned by GET /walkers/{walkerId}/available-slots", "example": "2026-07-15T09:00:00Z" },
          "durationMinutes": { "type": "integer", "enum": [30, 60, 90] },
          "specialInstructions": { "type": "string", "nullable": true, "description": "Optional instructions for the walker" }
        }
      },
      "BookingResponse": {
        "type": "object",
        "required": ["id", "clientUserId", "walkerId", "petId", "status", "slotStart", "durationMinutes", "createdAt", "updatedAt"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "clientUserId": { "type": "string", "format": "uuid" },
          "walkerId": { "type": "string", "format": "uuid", "description": "Walker entity id" },
          "petId": { "type": "string", "format": "uuid" },
          "status": { "type": "string", "enum": ["pending", "accepted", "rejected", "walker_cancelled", "owner_cancelled", "completed"] },
          "slotStart": { "type": "string", "format": "date-time" },
          "durationMinutes": { "type": "integer", "enum": [30, 60, 90] },
          "snapshotHourlyRate": { "type": "number", "format": "double", "nullable": true, "description": "Walker's rate captured at booking time" },
          "totalPrice": { "type": "number", "format": "double", "nullable": true, "description": "snapshotHourlyRate × durationMinutes / 60" },
          "specialInstructions": { "type": "string", "nullable": true },
          "walkSessionId": { "type": "string", "format": "uuid", "nullable": true, "description": "Set when booking is accepted" },
          "createdAt": { "type": "string", "format": "date-time" },
          "updatedAt": { "type": "string", "format": "date-time" }
        }
      },
      "AvailableSlotResponse": {
        "type": "object",
        "required": ["start", "end"],
        "properties": {
          "start": { "type": "string", "format": "date-time", "description": "Slot start (UTC)" },
          "end": { "type": "string", "format": "date-time", "description": "Slot end (UTC)" }
        }
      },
      "WalkerSummaryResponse": {
        "type": "object",
        "required": ["id", "fullName", "averageRating", "totalReviews", "distanceKm", "hasAvailability"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "fullName": { "type": "string" },
          "profilePhoto": { "type": "string", "nullable": true },
          "bio": { "type": "string", "nullable": true },
          "hourlyRate": { "type": "number", "format": "double", "nullable": true },
          "averageRating": { "type": "number", "format": "double" },
          "totalReviews": { "type": "integer" },
          "yearsOfExperience": { "type": "integer", "nullable": true },
          "serviceRadiusKm": { "type": "number", "format": "double", "nullable": true },
          "distanceKm": { "type": "number", "format": "double", "description": "Straight-line distance from query point" },
          "hasAvailability": { "type": "boolean", "description": "Always true — walkers without slots are filtered out" }
        }
      },
      "PagedWalkerSummary": {
        "type": "object",
        "required": ["items", "page", "pageSize", "totalCount"],
        "properties": {
          "items": { "type": "array", "items": { "$ref": "#/components/schemas/WalkerSummaryResponse" } },
          "page": { "type": "integer" },
          "pageSize": { "type": "integer" },
          "totalCount": { "type": "integer", "description": "Total matching walkers before pagination" }
        }
      },
      "WalkerDetailResponse": {
        "type": "object",
        "required": ["id", "userId", "fullName", "averageRating", "totalReviews", "completedWalks", "weeklyAvailability", "availableSlots"],
        "properties": {
          "id": { "type": "string", "format": "uuid" },
          "userId": { "type": "string", "format": "uuid" },
          "fullName": { "type": "string" },
          "profilePhoto": { "type": "string", "nullable": true },
          "bio": { "type": "string", "nullable": true },
          "hourlyRate": { "type": "number", "format": "double", "nullable": true },
          "serviceRadiusKm": { "type": "number", "format": "double", "nullable": true },
          "yearsOfExperience": { "type": "integer", "nullable": true },
          "workLocation": { "type": "string", "nullable": true },
          "averageRating": { "type": "number", "format": "double" },
          "totalReviews": { "type": "integer" },
          "completedWalks": { "type": "integer" },
          "weeklyAvailability": { "type": "array", "items": { "$ref": "#/components/schemas/AvailabilitySlotResponse" } },
          "availableSlots": { "type": "array", "items": { "$ref": "#/components/schemas/AvailableSlotResponse" }, "description": "Empty when date/durationMinutes not provided" }
        }
      }
    }
  }
}
""";
}
