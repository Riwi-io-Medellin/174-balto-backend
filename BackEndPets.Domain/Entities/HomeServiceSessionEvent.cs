namespace BackEndPets.Domain.Entities;

public sealed class HomeServiceSessionEvent
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}
