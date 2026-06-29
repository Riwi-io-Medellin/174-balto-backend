namespace BackEndPets.API.Endpoints;

public static class DocsEndpoints
{
    public static IEndpointRouteBuilder MapSwaggerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/swagger", () => Results.Content(SwaggerHtml, "text/html"))
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
      url: '/openapi/v1.json',
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
}
