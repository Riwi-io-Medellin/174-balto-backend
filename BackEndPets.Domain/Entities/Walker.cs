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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
