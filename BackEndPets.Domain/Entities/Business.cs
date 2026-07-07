namespace BackEndPets.Domain.Entities;

public sealed class Business
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public long Phone { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public bool SellsServices { get; set; }
    public bool SellsProducts { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}