using BackEndPets.Application.DTOs.Users;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Application.Services;

public sealed class UserService(ICrudRepository<User> userRepository) : IUserService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(ToResponse).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse?> CreateAsync(CreateUserRequest request)
    {
        if (!IsValid(request.FullName, request.Email, request.Role))
        {
            return null;
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Role = request.Role.Trim(),
            IsActive = request.IsActive
        };

        await userRepository.AddAsync(user);
        return ToResponse(user);
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null || !IsValid(request.FullName, request.Email, request.Role))
        {
            return null;
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.Role = request.Role.Trim();
        user.IsActive = request.IsActive;

        await userRepository.UpdateAsync(user);
        return ToResponse(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await userRepository.DeleteAsync(id);
    }

    private static bool IsValid(string fullName, string email, string role)
    {
        return !string.IsNullOrWhiteSpace(fullName)
            && !string.IsNullOrWhiteSpace(email)
            && !string.IsNullOrWhiteSpace(role);
    }

    private static UserResponse ToResponse(User user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.Role,
        user.IsActive,
        user.CreatedAt);
}
