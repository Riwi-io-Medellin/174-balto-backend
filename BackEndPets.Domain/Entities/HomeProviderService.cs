namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderService
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ServiceTypeId { get; set; }
    public decimal? Price { get; set; }
    public string PriceUnit { get; set; } = "flat";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
