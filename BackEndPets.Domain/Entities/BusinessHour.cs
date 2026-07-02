namespace BackEndPets.Domain.Entities;

public sealed class BusinessHour
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}