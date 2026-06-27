using BackEndPets.Application.DTOs.Chat;

namespace BackEndPets.Application.Interfaces;

public interface IChatService
{
    Task<(ChatMessageResponse? Response, string? ErrorCode)> SendMessageAsync(
        Guid userId, ChatMessageRequest request);
}