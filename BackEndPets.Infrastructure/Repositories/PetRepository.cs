using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class PetRepository(AppIdentityDbContext dbContext) : IPetRepository
{
    public async Task<Pet> CreateAsync(Pet pet)
    {
        pet.Id = Guid.NewGuid();
        pet.CreatedAt = DateTime.UtcNow;
        dbContext.Pets.Add(pet);
        await dbContext.SaveChangesAsync();
        return pet;
    }

    public Task<Pet?> GetByIdAsync(Guid id) =>
        dbContext.Pets.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyCollection<Pet>> GetByUserIdAsync(Guid userId) =>
        await dbContext.Pets
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Pet?> UpdateAsync(Pet pet)
    {
        var existing = await dbContext.Pets.FirstOrDefaultAsync(p => p.Id == pet.Id);
        if (existing is null) return null;

        existing.Name = pet.Name;
        existing.Species = pet.Species;
        existing.Breed = pet.Breed;
        existing.BirthDate = pet.BirthDate;
        existing.Description = pet.Description;
        existing.PhotoUrl = pet.PhotoUrl;

        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await dbContext.Pets.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return false;

        dbContext.Pets.Remove(existing);
        await dbContext.SaveChangesAsync();
        return true;
    }
}