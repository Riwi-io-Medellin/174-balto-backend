namespace BackEndPets.Domain.Entities;

public sealed class PaymentOrder
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ClientUserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "COP";

    /// <summary>pending | approved | declined | voided</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Referencia interna enviada a Wompi (=Id.ToString("N"))</summary>
    public string? ProviderReference { get; set; }

    /// <summary>ID de transacción retornado por Wompi en el webhook</summary>
    public string? ProviderTransactionId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
