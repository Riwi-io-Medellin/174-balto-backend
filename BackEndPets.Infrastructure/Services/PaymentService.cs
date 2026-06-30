using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BackEndPets.Application.DTOs.Payments;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BackEndPets.Infrastructure.Services;

public sealed class PaymentService(
    IPaymentOrderRepository paymentOrderRepository,
    IWalkBookingRepository bookingRepository,
    IConfiguration configuration) : IPaymentService
{
    // ── Configuración esperada en appsettings ─────────────────────────────────
    // Wompi:PublicKey        → llave pública Wompi
    // Wompi:EventSecret      → secreto de eventos del webhook
    // Wompi:CheckoutBaseUrl  → https://checkout.wompi.io/p  (prod) o sandbox
    // ─────────────────────────────────────────────────────────────────────────

    private string PublicKey    => configuration["Wompi:PublicKey"]!;
    private string EventSecret  => configuration["Wompi:EventSecret"]!;
    private string CheckoutBase => configuration["Wompi:CheckoutBaseUrl"]
                                   ?? "https://checkout.wompi.io/p";

    public async Task<(PaymentOrderResponse? Result, string? ErrorCode)> CreateOrderAsync(
        Guid clientUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)                          return (null, "BOOKING_NOT_FOUND");
        if (booking.ClientUserId != clientUserId)     return (null, "UNAUTHORIZED");
        if (booking.TotalPrice is null)               return (null, "AMOUNT_REQUIRED");

        // Idempotencia: si ya existe orden para este booking, la retornamos.
        var existing = await paymentOrderRepository.GetByBookingIdAsync(bookingId);
        if (existing is not null)
            return (Map(existing, BuildCheckoutUrl(existing)), null);

        var order = new PaymentOrder
        {
            BookingId    = bookingId,
            ClientUserId = clientUserId,
            Amount       = booking.TotalPrice.Value,
            Currency     = "COP",
            Status       = "pending"
        };

        var created = await paymentOrderRepository.CreateAsync(order);

        // Usamos el Id persistido como referencia hacia Wompi.
        created.ProviderReference = created.Id.ToString("N");
        await paymentOrderRepository.UpdateAsync(created);

        return (Map(created, BuildCheckoutUrl(created)), null);
    }

    public async Task<string?> HandleWebhookAsync(
        WompiWebhookPayload payload, string rawBody, string wompiSignatureHeader)
    {
        if (!VerifySignature(rawBody, wompiSignatureHeader))
            return "INVALID_SIGNATURE";

        var tx = payload.Data.Transaction;

        var order = await paymentOrderRepository.GetByProviderReferenceAsync(tx.Reference);
        if (order is null) return "ORDER_NOT_FOUND";

        order.ProviderTransactionId = tx.Id;
        order.Status = tx.Status.ToUpperInvariant() switch
        {
            "APPROVED" => "approved",
            "DECLINED" => "declined",
            "VOIDED"   => "voided",
            _          => order.Status
        };

        await paymentOrderRepository.UpdateAsync(order);
        return null;
    }

    public async Task<(PaymentOrderResponse? Result, string? ErrorCode)> GetByBookingIdAsync(
        Guid clientUserId, Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)                      return (null, "BOOKING_NOT_FOUND");
        if (booking.ClientUserId != clientUserId) return (null, "UNAUTHORIZED");

        var order = await paymentOrderRepository.GetByBookingIdAsync(bookingId);
        if (order is null) return (null, "ORDER_NOT_FOUND");

        return (Map(order, BuildCheckoutUrl(order)), null);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string BuildCheckoutUrl(PaymentOrder order)
    {
        var amountInCents = (long)(order.Amount * 100);
        return $"{CheckoutBase}?" +
               $"public-key={Uri.EscapeDataString(PublicKey)}" +
               $"&currency={order.Currency}" +
               $"&amount-in-cents={amountInCents}" +
               $"&reference={order.ProviderReference}";
    }

    /// <summary>
    /// Verifica la firma HMAC-SHA256 del evento Wompi.
    /// Wompi envía X-Event-Checksum = SHA256(rawBody + sentAt + eventSecret).
    /// Ref: https://docs.wompi.co/docs/colombia/webhooks/#validar-integridad
    /// </summary>
    private bool VerifySignature(string rawBody, string receivedChecksum)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var sentAt    = doc.RootElement.GetProperty("sent_at").GetInt64();
            var toHash    = $"{rawBody}{sentAt}{EventSecret}";
            var hash      = SHA256.HashData(Encoding.UTF8.GetBytes(toHash));
            var computed  = Convert.ToHexString(hash).ToLowerInvariant();
            return computed == receivedChecksum.ToLowerInvariant();
        }
        catch
        {
            return false;
        }
    }

    private static PaymentOrderResponse Map(PaymentOrder o, string checkoutUrl) => new(
        o.Id, o.BookingId, o.Amount, o.Currency, o.Status, checkoutUrl, o.CreatedAt);
}
