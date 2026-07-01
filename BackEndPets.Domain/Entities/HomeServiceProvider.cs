namespace BackEndPets.Domain.Entities;

public sealed class HomeServiceProvider
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public string? Description { get; set; }
    public string? Experience { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool IsAcceptingBookings { get; set; } = true;
    public int MaxConcurrentBookings { get; set; } = 1;
    public string? BaseLocation { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public string? RejectionReason { get; set; }
    public string? DocumentName { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
