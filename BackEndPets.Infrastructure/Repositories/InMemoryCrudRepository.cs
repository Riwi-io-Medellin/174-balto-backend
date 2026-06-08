using System.Collections.Concurrent;
using BackEndPets.Domain.Common;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class InMemoryCrudRepository<T> : ICrudRepository<T> where T : EntityBase, new()
{
    private readonly ConcurrentDictionary<Guid, T> storage = new();

    public Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        IReadOnlyCollection<T> items = storage.Values.OrderBy(item => item.CreatedAt).ToList();
        return Task.FromResult(items);
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        storage.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task AddAsync(T entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.CreatedAt = DateTimeOffset.UtcNow;
        storage[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(T entity)
    {
        if (!storage.ContainsKey(entity.Id))
        {
            return Task.FromResult(false);
        }

        storage[entity.Id] = entity;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id) => Task.FromResult(storage.TryRemove(id, out _));
}
