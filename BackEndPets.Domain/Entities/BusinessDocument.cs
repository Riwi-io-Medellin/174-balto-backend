namespace BackEndPets.Domain.Entities;

public sealed class BusinessDocument
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}