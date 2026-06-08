using BackEndPets.Application.DTOs.Users;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Application.Services;

public sealed class UserService(ICrudRepository<User> userRepository) : IUserService
{
    private static readonly string[] ValidIdTypes = ["CC", "CE", "Passport", "TI"];

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
        if (!IsValid(request) || await HasDuplicateAsync(request.Email, request.IdNumber))
        {
            return null;
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            IdNumber = request.IdNumber.Trim(),
            IdType = request.IdType.Trim(),
            Location = NormalizeOptional(request.Location),
            Address = NormalizeOptional(request.Address),
            Phone = request.Phone,
            PhoneExtra = request.PhoneExtra,
            PhotoUrl = NormalizeOptional(request.PhotoUrl)
        };

        await userRepository.AddAsync(user);
        return ToResponse(user);
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null || !IsValid(request) || await HasDuplicateAsync(request.Email, request.IdNumber, id))
        {
            return null;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim();
        user.IdNumber = request.IdNumber.Trim();
        user.IdType = request.IdType.Trim();
        user.Location = NormalizeOptional(request.Location);
        user.Address = NormalizeOptional(request.Address);
        user.Phone = request.Phone;
        user.PhoneExtra = request.PhoneExtra;
        user.PhotoUrl = NormalizeOptional(request.PhotoUrl);

        await userRepository.UpdateAsync(user);
        return ToResponse(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await userRepository.DeleteAsync(id);
    }

    private static bool IsValid(CreateUserRequest request)
    {
        return IsValid(request.FirstName, request.LastName, request.Email, request.IdNumber, request.IdType, request.Phone);
    }

    private static bool IsValid(UpdateUserRequest request)
    {
        return IsValid(request.FirstName, request.LastName, request.Email, request.IdNumber, request.IdType, request.Phone);
    }

    private static bool IsValid(string firstName, string lastName, string email, string idNumber, string idType, long phone)
    {
        return !string.IsNullOrWhiteSpace(firstName)
            && !string.IsNullOrWhiteSpace(lastName)
            && !string.IsNullOrWhiteSpace(email)
            && !string.IsNullOrWhiteSpace(idNumber)
            && !string.IsNullOrWhiteSpace(idType)
            && ValidIdTypes.Contains(idType.Trim(), StringComparer.OrdinalIgnoreCase)
            && phone != 0;
    }

    private async Task<bool> HasDuplicateAsync(string email, string idNumber, Guid? currentId = null)
    {
        var users = await userRepository.GetAllAsync();
        return users.Any(user =>
            (!currentId.HasValue || user.Id != currentId.Value)
            && (string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(user.IdNumber, idNumber.Trim(), StringComparison.OrdinalIgnoreCase)));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static UserResponse ToResponse(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.IdNumber,
        user.IdType,
        user.Location,
        user.Address,
        user.Phone,
        user.PhoneExtra,
        user.PhotoUrl,
        user.CreatedAt);
}
