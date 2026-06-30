using System.Security.Claims;
using BackEndPets.Application.DTOs.Chat;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chat", async (
                ChatMessageRequest request,
                HttpContext ctx,
                IChatService service) =>
            {
                var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                var (response, errorCode) = await service.SendMessageAsync(userId, request);
                if (response is not null) return Results.Ok(response);

                return errorCode switch
                {
                    "VALIDATION_FAILED" => Results.BadRequest(new ApiErrorResponse("Message is required.", "VALIDATION_FAILED")),
                    "AI_UNAVAILABLE"    => Results.StatusCode(503),
                    _                   => ResultsExtensions.UnhandledError()
                };
            })
            .WithTags("Chat")
            .WithName("Chat")
            .WithSummary("Send a message to the Balto assistant")
            .RequireAuthorization()
            .Produces<ChatMessageResponse>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}