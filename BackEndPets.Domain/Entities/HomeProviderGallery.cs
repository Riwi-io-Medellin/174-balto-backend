namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderGallery
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
