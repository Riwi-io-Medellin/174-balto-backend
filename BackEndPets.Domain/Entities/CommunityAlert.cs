namespace BackEndPets.Domain.Entities;

public sealed class CommunityAlert
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public string AlertType { get; set; } = "lost";
    public string PetName { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? LastSeenLocation { get; set; }
    public string? EvidenceUrl { get; set; }
    public string Status { get; set; } = "active";
    public string? ModerationReason { get; set; }
    public Guid? ModeratedByUserId { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
