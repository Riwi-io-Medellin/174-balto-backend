namespace BackEndPets.Domain.Entities;

public sealed class Pet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Weight { get; set; }
    public bool IsLost { get; set; }
    public double? LostLatitude { get; set; }
    public double? LostLongitude { get; set; }
    public DateTime? LostAt { get; set; }
    public DateTime CreatedAt { get; set; }
}