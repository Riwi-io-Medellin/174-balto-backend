using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class PetHistoryService(
    IPetRepository petRepository,
    IPetHistoryRepository historyRepository) : IPetHistoryService
{
    public async Task<(PetHistoryResponse? History, string? ErrorCode)> CreateAsync(
        Guid userId, Guid petId, CreatePetHistoryRequest request)
    {
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != userId) return (null, "UNAUTHORIZED");

        var history = new PetHistory
        {
            PetId = petId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DocumentUrl = request.DocumentUrl?.Trim()
        };

        var created = await historyRepository.CreateAsync(history);
        return (MapResponse(created), null);
    }

    public async Task<(IReadOnlyCollection<PetHistoryResponse>? Histories, string? ErrorCode)> GetByPetIdAsync(
        Guid userId, Guid petId)
    {
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != userId) return (null, "UNAUTHORIZED");

        var histories = await historyRepository.GetByPetIdAsync(petId);
        return (histories.Select(MapResponse).ToList(), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid historyId)
    {
        var history = await historyRepository.GetByIdAsync(historyId);
        if (history is null) return (false, "HISTORY_NOT_FOUND");

        var pet = await petRepository.GetByIdAsync(history.PetId);
        if (pet is null || pet.UserId != userId) return (false, "UNAUTHORIZED");

        await historyRepository.DeleteAsync(historyId);
        return (true, null);
    }

    private static PetHistoryResponse MapResponse(PetHistory h) =>
        new(h.Id, h.PetId, h.Title, h.Description, h.DocumentUrl, h.CreatedAt);
}