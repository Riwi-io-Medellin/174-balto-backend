using BackEndPets.Application.DTOs.Payments;

namespace BackEndPets.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Crea una PaymentOrder vinculada al booking y devuelve la URL de checkout de Wompi.
    /// ErrorCodes: BOOKING_NOT_FOUND | UNAUTHORIZED | PAYMENT_ALREADY_EXISTS | AMOUNT_REQUIRED
    /// </summary>
    Task<(PaymentOrderResponse? Result, string? ErrorCode)> CreateOrderAsync(
        Guid clientUserId, Guid bookingId);

    /// <summary>
    /// Procesa el webhook de Wompi. Actualiza el estado de la PaymentOrder.
    /// Si es APPROVED, no cambia el booking (sigue pending hasta que el walker acepte).
    /// ErrorCodes: ORDER_NOT_FOUND | INVALID_SIGNATURE
    /// </summary>
    Task<string?> HandleWebhookAsync(WompiWebhookPayload payload, string rawBody, string wompiSignatureHeader);

    /// <summary>
    /// Devuelve la orden de pago asociada a un booking.
    /// ErrorCodes: ORDER_NOT_FOUND | UNAUTHORIZED
    /// </summary>
    Task<(PaymentOrderResponse? Result, string? ErrorCode)> GetByBookingIdAsync(
        Guid clientUserId, Guid bookingId);
}
