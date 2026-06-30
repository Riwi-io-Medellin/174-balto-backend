using System.Net;
using System.Text.Json;
using BackEndPets.Application.DTOs.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace BackEndPets.API.Middleware;

/// <summary>
/// Captura cualquier excepción no manejada y retorna siempre un ApiErrorResponse estructurado.
/// Registrar en Program.cs con: app.UseExceptionHandler(...)  ANTES de UseAuthentication.
/// </summary>
public static class GlobalExceptionHandler
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception        = exceptionFeature?.Error;

                var logger = context.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                logger.LogError(exception,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new ApiErrorResponse(
                    "An unexpected error occurred. Please try again later.",
                    "INTERNAL_SERVER_ERROR");

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            });
        });

        return app;
    }
}
