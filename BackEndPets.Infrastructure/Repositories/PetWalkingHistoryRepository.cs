using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class PetWalkingHistoryRepository(AppIdentityDbContext dbContext) : IPetWalkingHistoryRepository
{
    public Task<PetWalkingHistory?> GetByIdAsync(Guid historyId) =>
        dbContext.PetWalkingHistories.FirstOrDefaultAsync(h => h.Id == historyId);
}
