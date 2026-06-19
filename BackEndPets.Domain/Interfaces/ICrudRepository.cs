using BackEndPets.Domain.Common;

namespace BackEndPets.Domain.Interfaces;

public interface ICrudRepository<T> where T : EntityBase
{
    Task<IReadOnlyCollection<T>> GetAllAsync();

    Task<T?> GetByIdAsync(Guid id);

    Task AddAsync(T entity);

    Task<bool> UpdateAsync(T entity);

    Task<bool> DeleteAsync(Guid id);
}
