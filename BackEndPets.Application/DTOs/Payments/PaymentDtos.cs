namespace BackEndPets.Application.DTOs.Payments;

/// <summary>Respuesta al crear una orden de pago. Contiene la URL de checkout de Wompi.</summary>
public sealed record PaymentOrderResponse(
    Guid Id,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string Status,
    string CheckoutUrl,
    DateTime CreatedAt);

/// <summary>Payload del webhook de Wompi (evento transaction.updated).</summary>
public sealed record WompiWebhookPayload(
    string Event,
    WompiWebhookData Data,
    long SentAt,
    string Signature);

public sealed record WompiWebhookData(WompiTransaction Transaction);

public sealed record WompiTransaction(
    string Id,
    string Reference,
    string Status,        // APPROVED | DECLINED | VOIDED | ERROR
    decimal AmountInCents,
    string Currency);
