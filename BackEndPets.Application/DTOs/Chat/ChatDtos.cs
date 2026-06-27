namespace BackEndPets.Application.DTOs.Chat;

public sealed record ChatMessageRequest(
    string Message,
    IReadOnlyList<ChatTurn>? History = null);

public sealed record ChatTurn(
    string Role,
    string Content);

public sealed record ChatMessageResponse(
    string Reply);