namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderSpecialty
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
