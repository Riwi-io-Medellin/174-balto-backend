namespace BackEndPets.Domain.Entities;

public sealed class WalkerGallery
{
    public Guid Id { get; set; }
    public Guid WalkerId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}