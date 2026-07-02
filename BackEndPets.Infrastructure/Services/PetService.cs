using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Services;

public sealed class PetService(
    IPetRepository petRepository,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService) : IPetService
{
    public async Task<PetResponse> CreateAsync(Guid userId, CreatePetRequest request)
    {
        var pet = new Pet
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Species = request.Species?.Trim(),
            Breed = request.Breed?.Trim(),
            BirthDate = request.BirthDate,
            Description = request.Description?.Trim(),
            Weight = request.Weight,
            PhotoUrl = request.PhotoUrl?.Trim()
        };

        var created = await petRepository.CreateAsync(pet);
        return MapResponse(created);
    }

    public async Task<PetResponse?> GetByIdAsync(Guid id)
    {
        var pet = await petRepository.GetByIdAsync(id);
        return pet is null ? null : MapResponse(pet);
    }

    public async Task<PagedResult<PetResponse>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var all = await petRepository.GetByUserIdAsync(userId);
        var totalCount = all.Count;
        var paged = all
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapResponse)
            .ToList();
        return new PagedResult<PetResponse>(paged, page, pageSize, totalCount);
    }

    public async Task<(PetResponse? Pet, string? ErrorCode)> UpdateAsync(Guid userId, Guid petId, UpdatePetRequest request)
    {
        var existing = await petRepository.GetByIdAsync(petId);
        if (existing is null) return (null, "PET_NOT_FOUND");
        if (existing.UserId != userId) return (null, "UNAUTHORIZED");

        existing.Name = request.Name.Trim();
        existing.Species = request.Species?.Trim();
        existing.Breed = request.Breed?.Trim();
        existing.BirthDate = request.BirthDate;
        existing.Description = request.Description?.Trim();
        existing.Weight = request.Weight;
        existing.PhotoUrl = request.PhotoUrl?.Trim();

        var updated = await petRepository.UpdateAsync(existing);
        return (MapResponse(updated!), null);
    }

    public async Task<(bool Success, string? ErrorCode)> DeleteAsync(Guid userId, Guid petId)
    {
        var existing = await petRepository.GetByIdAsync(petId);
        if (existing is null) return (false, "PET_NOT_FOUND");
        if (existing.UserId != userId) return (false, "UNAUTHORIZED");

        await petRepository.DeleteAsync(petId);
        return (true, null);
    }

    private static PetResponse MapResponse(Pet p) =>
        new(p.Id, p.UserId, p.Name, p.Species, p.Breed, p.BirthDate, p.Description, p.PhotoUrl, p.Weight, p.CreatedAt,
            p.IsLost, p.LostLatitude, p.LostLongitude, p.LostAt);
    
    public async Task<(PetResponse? Pet, string? ErrorCode)> ReportLostAsync(
        Guid userId, Guid petId, ReportPetLostRequest request)
    {
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != userId) return (null, "UNAUTHORIZED");

        pet.IsLost = true;
        pet.LostLatitude = request.LostLatitude;
        pet.LostLongitude = request.LostLongitude;
        pet.LostAt = DateTime.UtcNow;
        await petRepository.UpdateAsync(pet);

        var nearbyUsers = userManager.Users
            .Where(u => u.Id != userId && u.Latitude != null && u.Longitude != null)
            .ToList()
            .Where(u => GeoUtils.Haversine(request.LostLatitude, request.LostLongitude,
                u.Latitude!.Value, u.Longitude!.Value) <= 3.0);

        foreach (var u in nearbyUsers)
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: u.Id,
                Type: "lost_pet",
                Title: $"{pet.Name} got lost near you",
                Body: "Tap to view the location and help bring them home.",
                EntityId: pet.Id,
                EntityType: "pet"));
        }

        return (MapResponse(pet), null);
    }

    public async Task<(PetResponse? Pet, string? ErrorCode)> MarkFoundAsync(Guid userId, Guid petId)
    {
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet is null) return (null, "PET_NOT_FOUND");
        if (pet.UserId != userId) return (null, "UNAUTHORIZED");

        pet.IsLost = false;
        await petRepository.UpdateAsync(pet);
        return (MapResponse(pet), null);
    }
}