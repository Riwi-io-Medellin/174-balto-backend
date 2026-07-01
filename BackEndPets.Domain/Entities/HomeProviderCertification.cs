namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderCertification
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? ServiceTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IssuingOrganization { get; set; }
    public string? CredentialNumber { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
