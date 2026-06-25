namespace BackEndPets.Domain.Entities;

public sealed class PetWalkingHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WalkerId { get; set; }
}
