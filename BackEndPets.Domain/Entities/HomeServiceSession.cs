namespace BackEndPets.Domain.Entities;

public sealed class HomeServiceSession
{
    public Guid Id { get; set; }
    public Guid? ProviderId { get; set; }
    public Guid? BookingId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public double? TotalDistanceMeters { get; set; }
    public int? TotalDurationSeconds { get; set; }
}
