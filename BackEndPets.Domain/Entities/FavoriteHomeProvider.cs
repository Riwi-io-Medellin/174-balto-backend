namespace BackEndPets.Domain.Entities;

public sealed class FavoriteHomeProvider
{
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
