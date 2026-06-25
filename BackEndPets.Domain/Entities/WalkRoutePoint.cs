namespace BackEndPets.Domain.Entities;

public sealed class WalkRoutePoint
{
    public Guid Id { get; set; }
    public Guid WalkSessionId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}
