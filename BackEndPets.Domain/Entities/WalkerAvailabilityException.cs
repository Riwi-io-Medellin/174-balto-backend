namespace BackEndPets.Domain.Entities;

public sealed class WalkerAvailabilityException
{
    public Guid Id { get; set; }
    public Guid WalkerId { get; set; }
    public DateOnly Date { get; set; }
    /// <summary>
    /// When true the walker is unavailable all day; StartTime and EndTime are null.
    /// When false the walker has an alternate schedule on this date and StartTime/EndTime are required.
    /// </summary>
    public bool IsUnavailable { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
