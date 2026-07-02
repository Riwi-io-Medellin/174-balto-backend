namespace BackEndPets.Domain.Entities;

public sealed class HomeServiceBooking
{
    public Guid Id { get; set; }
    public Guid ClientUserId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public Guid PetId { get; set; }
    public string Status { get; set; } = "pending"; // pending|accepted|rejected|provider_cancelled|client_cancelled|in_progress|completed
    public DateTime SlotStart { get; set; }
    public int DurationMinutes { get; set; }
    public decimal? SnapshotPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? ServiceAddress { get; set; }
    public double? ServiceLatitude { get; set; }
    public double? ServiceLongitude { get; set; }
    public string? SpecialInstructions { get; set; }
    public Guid? HomeServiceSessionId { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
