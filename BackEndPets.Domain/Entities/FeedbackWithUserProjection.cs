namespace BackEndPets.Domain.Entities;

public sealed record FeedbackWithUserProjection(
    Feedback Feedback,
    string FirstName,
    string LastName,
    string? PhotoUrl);
