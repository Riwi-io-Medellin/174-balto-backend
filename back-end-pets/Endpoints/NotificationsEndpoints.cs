using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", async (
            bool? unreadOnly,
            int page,
            int pageSize,
            HttpContext ctx,
            INotificationService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var result = await service.GetMyNotificationsAsync(userId, unreadOnly, page, pageSize);
            return Results.Ok(result);
        })
        .WithName("GetMyNotifications")
        .WithSummary("Get all notifications for the current user")
        .Produces<NotificationSummaryResponse>(StatusCodes.Status200OK);

        group.MapGet("/unread-count", async (
            HttpContext ctx,
            INotificationService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var count = await service.GetUnreadCountAsync(userId);
            return Results.Ok(new { UnreadCount = count });
        })
        .WithName("GetUnreadCount")
        .WithSummary("Get the unread notification count for the current user")
        .Produces(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/read", async (
            Guid id,
            HttpContext ctx,
            INotificationService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (notification, errorCode) = await service.MarkAsReadAsync(userId, id);
            return errorCode switch
            {
                "NOTIFICATION_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Notification not found.", "NOTIFICATION_NOT_FOUND")),
                "UNAUTHORIZED" => Results.Json(
                    new ApiErrorResponse("This notification does not belong to you.", "UNAUTHORIZED"),
                    statusCode: StatusCodes.Status403Forbidden),
                _ => Results.Ok(notification)
            };
        })
        .WithName("MarkNotificationAsRead")
        .WithSummary("Mark a notification as read")
        .Produces<NotificationResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/read-all", async (
            HttpContext ctx,
            INotificationService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var count = await service.MarkAllAsReadAsync(userId);
            return Results.Ok(new { MarkedAsRead = count });
        })
        .WithName("MarkAllNotificationsAsRead")
        .WithSummary("Mark all notifications as read for the current user")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}