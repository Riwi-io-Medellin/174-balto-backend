namespace BackEndPets.Domain.Entities;

public sealed class Feedback
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty; // "walker" | "business"
    public string? Comment { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}