namespace BackEndPets.Domain.Entities;

public sealed class BusinessHourException
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsUnavailable { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}