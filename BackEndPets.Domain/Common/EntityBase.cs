namespace BackEndPets.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
