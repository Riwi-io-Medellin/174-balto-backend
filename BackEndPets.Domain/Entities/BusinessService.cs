namespace BackEndPets.Domain.Entities;

public sealed class BusinessService
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}