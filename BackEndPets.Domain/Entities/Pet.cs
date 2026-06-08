using BackEndPets.Domain.Common;

namespace BackEndPets.Domain.Entities;

public sealed class Pet : EntityBase
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string? Breed { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Description { get; set; }

    public string? PhotoUrl { get; set; }
}
