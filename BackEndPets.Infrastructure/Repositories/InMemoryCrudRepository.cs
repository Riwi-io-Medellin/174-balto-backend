using System.Collections.Concurrent;
using BackEndPets.Domain.Common;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class InMemoryCrudRepository<T> : ICrudRepository<T> where T : EntityBase, new()
{
    private readonly ConcurrentDictionary<Guid, T> _storage = new();

    public Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        IReadOnlyCollection<T> items = _storage.Values.OrderBy(item => item.CreatedAt).ToList();
        return Task.FromResult(items);
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        _storage.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task AddAsync(T entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.CreatedAt = DateTimeOffset.UtcNow;
        _storage[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(T entity)
    {
        if (!_storage.ContainsKey(entity.Id))
        {
            return Task.FromResult(false);
        }

        _storage[entity.Id] = entity;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id) => Task.FromResult(_storage.TryRemove(id, out _));
}
