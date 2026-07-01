namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderDocument
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
