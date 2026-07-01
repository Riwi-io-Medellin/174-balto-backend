namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderAvailabilityException
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DateOnly Date { get; set; }
    /// <summary>
    /// When true the provider is unavailable all day; StartTime and EndTime are null.
    /// When false the provider has an alternate schedule on this date and StartTime/EndTime are required.
    /// </summary>
    public bool IsUnavailable { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
