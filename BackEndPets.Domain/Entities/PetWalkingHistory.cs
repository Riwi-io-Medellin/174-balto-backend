namespace BackEndPets.Domain.Entities;

public sealed class PetWalkingHistory
{
    public Guid Id { get; set; }
    public Guid? WalkSessionId { get; set; }
    public Guid UserId { get; set; }
    public Guid PetId { get; set; }
    public Guid WalkerId { get; set; }
    public decimal? Cost { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}