using System.Security.Claims;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Payments;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Autenticado ───────────────────────────────────────────────────────

        var payments = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        // POST /api/payments/bookings/{bookingId}
        // Crea (o retorna) la orden de pago para un booking.
        payments.MapPost("/bookings/{bookingId:guid}", async (
            Guid bookingId,
            HttpContext ctx,
            IPaymentService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.CreateOrderAsync(userId, bookingId);
            return errorCode switch
            {
                "BOOKING_NOT_FOUND"  => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"       => Results.Json(new ApiErrorResponse("You do not own this booking.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "AMOUNT_REQUIRED"    => Results.BadRequest(new ApiErrorResponse("Booking has no price set.", "AMOUNT_REQUIRED")),
                _ when result is not null => Results.Created($"/api/payments/bookings/{bookingId}", result),
                _                         => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("CreatePaymentOrder")
        .WithSummary("Create or retrieve the payment order for a booking")
        .Produces<PaymentOrderResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // GET /api/payments/bookings/{bookingId}
        // Consulta el estado de la orden de pago.
        payments.MapGet("/bookings/{bookingId:guid}", async (
            Guid bookingId,
            HttpContext ctx,
            IPaymentService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var (result, errorCode) = await service.GetByBookingIdAsync(userId, bookingId);
            return errorCode switch
            {
                "BOOKING_NOT_FOUND" => Results.NotFound(new ApiErrorResponse("Booking not found.", "BOOKING_NOT_FOUND")),
                "UNAUTHORIZED"      => Results.Json(new ApiErrorResponse("You do not own this booking.", "UNAUTHORIZED"), statusCode: StatusCodes.Status403Forbidden),
                "ORDER_NOT_FOUND"   => Results.NotFound(new ApiErrorResponse("Payment order not found.", "ORDER_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _                         => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetPaymentOrder")
        .WithSummary("Get the payment order status for a booking")
        .Produces<PaymentOrderResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        // ── Webhook (sin autenticación) ───────────────────────────────────────

        // POST /api/payments/wompi/webhook
        // Wompi llama a este endpoint cuando cambia el estado de una transacción.
        app.MapPost("/api/payments/wompi/webhook", async (
            WompiWebhookPayload payload,
            HttpContext ctx,
            IPaymentService service) =>
        {
            // Leemos el body crudo para verificar la firma.
            ctx.Request.EnableBuffering();
            ctx.Request.Body.Position = 0;
            using var reader = new StreamReader(ctx.Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();

            var signature = ctx.Request.Headers["X-Event-Checksum"].FirstOrDefault() ?? string.Empty;

            var errorCode = await service.HandleWebhookAsync(payload, rawBody, signature);
            return errorCode switch
            {
                "INVALID_SIGNATURE" => Results.Json(new ApiErrorResponse("Invalid signature.", "INVALID_SIGNATURE"), statusCode: StatusCodes.Status401Unauthorized),
                "ORDER_NOT_FOUND"   => Results.NotFound(new ApiErrorResponse("Order not found.", "ORDER_NOT_FOUND")),
                null                => Results.Ok(),
                _                   => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("WompiWebhook")
        .WithSummary("Wompi event webhook — no authentication required")
        .WithTags("Payments")
        .Produces(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
