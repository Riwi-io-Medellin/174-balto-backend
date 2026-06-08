using BackEndPets.Application.DTOs.Users;

namespace BackEndPets.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync();

    Task<UserResponse?> GetByIdAsync(Guid id);

    Task<UserResponse?> CreateAsync(CreateUserRequest request);

    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request);

    Task<bool> DeleteAsync(Guid id);
}
