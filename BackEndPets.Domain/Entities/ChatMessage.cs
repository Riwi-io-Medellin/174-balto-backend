namespace BackEndPets.Domain.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; set; }
    public Guid WalkSessionId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
