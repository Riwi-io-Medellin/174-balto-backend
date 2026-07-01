namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderServiceArea
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string? Label { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal RadiusKm { get; set; }
    public DateTime CreatedAt { get; set; }
}
