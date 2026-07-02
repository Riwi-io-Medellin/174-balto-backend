using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeServiceTypeRepository(AppIdentityDbContext dbContext) : IHomeServiceTypeRepository
{
    public async Task<IReadOnlyCollection<HomeServiceType>> GetAllActiveAsync() =>
        await dbContext.HomeServiceTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public Task<HomeServiceType?> GetByIdAsync(Guid id) =>
        dbContext.HomeServiceTypes.FirstOrDefaultAsync(t => t.Id == id);

    public Task<HomeServiceType?> GetByCodeAsync(string code) =>
        dbContext.HomeServiceTypes.FirstOrDefaultAsync(t => t.Code == code);
}
