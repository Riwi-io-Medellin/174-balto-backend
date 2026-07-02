namespace BackEndPets.Domain.Entities;

public sealed class WalkSessionMedia
{
    public Guid Id { get; set; }
    public Guid WalkSessionId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "photo"; // "photo" | "video"
    public DateTime UploadedAt { get; set; }
}
