using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class DeviceTokenRepository(AppIdentityDbContext dbContext) : IDeviceTokenRepository
{
    public async Task RegisterAsync(Guid userId, string token, string platform)
    {
        var existing = await dbContext.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing is not null)
        {
            existing.UserId = userId;
            existing.Platform = platform;
            existing.LastSeenAt = DateTime.UtcNow;
        }
        else
        {
            dbContext.DeviceTokens.Add(new DeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                Platform = platform,
                CreatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            });
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(string token)
    {
        var existing = await dbContext.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing is null) return;
        dbContext.DeviceTokens.Remove(existing);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<string>> GetTokensByUserIdAsync(Guid userId) =>
        await dbContext.DeviceTokens
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .ToListAsync();

    public async Task RemoveTokensAsync(IReadOnlyCollection<string> tokens)
    {
        if (tokens.Count == 0) return;
        var existing = await dbContext.DeviceTokens.Where(t => tokens.Contains(t.Token)).ToListAsync();
        dbContext.DeviceTokens.RemoveRange(existing);
        await dbContext.SaveChangesAsync();
    }
}
