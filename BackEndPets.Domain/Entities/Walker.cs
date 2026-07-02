namespace BackEndPets.Domain.Entities;

public sealed class Walker
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool Available { get; set; } = false;
    public string? WorkLocation { get; set; }
    public string? Experience { get; set; }
    public string? Description { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public string? DocumentName { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Bio { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? ServiceRadiusKm { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool IsAcceptingBookings { get; set; } = false;
    public double? WorkLatitude { get; set; }
    public double? WorkLongitude { get; set; }
    public int? MaxDogs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
