namespace BackEndPets.Domain.Entities;

public sealed class WalkSession
{
    public Guid Id { get; set; }
    public Guid PetWalkingHistoryId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
