namespace BackEndPets.Domain.Entities;

public sealed class WalkBooking
{
    public Guid Id { get; set; }
    public Guid ClientUserId { get; set; }         // pet owner's user id
    public Guid WalkerId { get; set; }              // Walker entity id (not user id)
    public Guid PetId { get; set; }
    public string Status { get; set; } = "pending"; // pending | accepted | rejected | cancelled | completed
    public DateTime SlotStart { get; set; }         // requested walk start time (UTC)
    public int DurationMinutes { get; set; }        // 30 | 60 | 90
    public decimal? SnapshotHourlyRate { get; set; } // walker's rate at booking time
    public decimal? TotalPrice { get; set; }
    public bool IsExclusive { get; set; } = false;
    public string? SpecialInstructions { get; set; }
    public Guid? WalkSessionId { get; set; }        // set when status → accepted
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
