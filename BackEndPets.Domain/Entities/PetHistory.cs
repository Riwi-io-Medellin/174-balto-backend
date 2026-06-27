namespace BackEndPets.Domain.Entities;

public sealed class PetHistory
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}