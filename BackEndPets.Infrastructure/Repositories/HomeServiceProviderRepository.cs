using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class HomeServiceProviderRepository(AppIdentityDbContext dbContext) : IHomeServiceProviderRepository
{
    public Task<HomeServiceProvider?> GetByUserIdAsync(Guid userId) =>
        dbContext.HomeServiceProviders.FirstOrDefaultAsync(p => p.UserId == userId);

    public Task<bool> ExistsForUserAsync(Guid userId) =>
        dbContext.HomeServiceProviders.AnyAsync(p => p.UserId == userId);

    public async Task<HomeServiceProvider> CreateAsync(HomeServiceProvider provider)
    {
        provider.Id = Guid.NewGuid();
        provider.CreatedAt = DateTime.UtcNow;
        provider.UpdatedAt = DateTime.UtcNow;
        dbContext.HomeServiceProviders.Add(provider);
        await dbContext.SaveChangesAsync();
        return provider;
    }

    public async Task<IReadOnlyCollection<HomeServiceProvider>> GetAllAsync(
        bool? isAcceptingBookings = null, string? baseLocation = null)
    {
        var query = dbContext.HomeServiceProviders.AsQueryable();

        if (isAcceptingBookings.HasValue)
            query = query.Where(p => p.IsAcceptingBookings == isAcceptingBookings.Value);

        if (!string.IsNullOrWhiteSpace(baseLocation))
            query = query.Where(p => p.BaseLocation != null &&
                                     p.BaseLocation.ToLower().Contains(baseLocation.ToLower()));

        return await query.OrderBy(p => p.CreatedAt).ToListAsync();
    }

    public Task<HomeServiceProvider?> GetByIdAsync(Guid id) =>
        dbContext.HomeServiceProviders.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<HomeServiceProvider> UpdateAsync(HomeServiceProvider provider)
    {
        provider.UpdatedAt = DateTime.UtcNow;
        dbContext.HomeServiceProviders.Update(provider);
        await dbContext.SaveChangesAsync();
        return provider;
    }

    public async Task<IReadOnlyCollection<HomeServiceProviderUserProjection>> GetAllWithUserAsync(
        bool? isAcceptingBookings = null, string? baseLocation = null)
    {
        var query = dbContext.HomeServiceProviders
            .Where(p => p.VerificationStatus == "approved")
            .AsQueryable();

        if (isAcceptingBookings.HasValue)
            query = query.Where(p => p.IsAcceptingBookings == isAcceptingBookings.Value);

        if (!string.IsNullOrWhiteSpace(baseLocation))
            query = query.Where(p => p.BaseLocation != null &&
                                     p.BaseLocation.ToLower().Contains(baseLocation.ToLower()));

        return await query
            .Join(dbContext.Users,
                p => p.UserId,
                u => u.Id,
                (p, u) => new { Provider = p, User = u })
            .OrderBy(x => x.Provider.CreatedAt)
            .Select(x => new HomeServiceProviderUserProjection(
                x.Provider, x.User.FirstName, x.User.LastName, x.User.PhotoUrl))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<HomeServiceProviderUserProjection>> GetApprovedAcceptingWithUserAsync() =>
        await dbContext.HomeServiceProviders
            .Where(p => p.VerificationStatus == "approved" && p.IsAcceptingBookings)
            .Join(dbContext.Users,
                p => p.UserId,
                u => u.Id,
                (p, u) => new HomeServiceProviderUserProjection(p, u.FirstName, u.LastName, u.PhotoUrl))
            .ToListAsync();

    public async Task<HomeServiceProviderUserProjection?> GetByIdWithUserAsync(Guid id) =>
        await dbContext.HomeServiceProviders
            .Where(p => p.Id == id)
            .Join(dbContext.Users,
                p => p.UserId,
                u => u.Id,
                (p, u) => new HomeServiceProviderUserProjection(p, u.FirstName, u.LastName, u.PhotoUrl))
            .FirstOrDefaultAsync();
}
