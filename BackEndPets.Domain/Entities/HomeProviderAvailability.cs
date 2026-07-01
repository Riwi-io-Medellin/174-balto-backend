namespace BackEndPets.Domain.Entities;

public sealed class HomeProviderAvailability
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public int DayOfWeek { get; set; } // 0 = Sunday … 6 = Saturday
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
