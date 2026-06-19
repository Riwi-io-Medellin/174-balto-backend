using BackEndPets.Application.DTOs.Users;
using BackEndPets.Application.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Services;

public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    AppIdentityDbContext dbContext) : IUserService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync()
    {
        var users = await dbContext.Users
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(ToResponse).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse?> CreateAsync(CreateUserRequest request)
    {
        if (!IsValid(request.FirstName, request.LastName, request.Email, request.IdNumber, request.IdType))
        {
            return null;
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IdNumber = request.IdNumber.Trim(),
            IdType = request.IdType.Trim(),
            Phone = request.Phone,
            PhoneExtra = request.PhoneExtra,
            Location = request.Location?.Trim(),
            Address = request.Address?.Trim(),
            PhotoUrl = request.PhotoUrl?.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        return result.Succeeded ? ToResponse(user) : null;
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null || !IsValid(request.FirstName, request.LastName, user.Email ?? string.Empty, request.IdNumber, request.IdType))
        {
            return null;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.IdNumber = request.IdNumber.Trim();
        user.IdType = request.IdType.Trim();
        user.Phone = request.Phone;
        user.PhoneExtra = request.PhoneExtra;
        user.Location = request.Location?.Trim();
        user.Address = request.Address?.Trim();
        user.PhotoUrl = request.PhotoUrl?.Trim();

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? ToResponse(user) : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return false;
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    private static bool IsValid(string firstName, string lastName, string email, string idNumber, string idType)
    {
        return !string.IsNullOrWhiteSpace(firstName)
            && !string.IsNullOrWhiteSpace(lastName)
            && !string.IsNullOrWhiteSpace(email)
            && !string.IsNullOrWhiteSpace(idNumber)
            && !string.IsNullOrWhiteSpace(idType);
    }

    private static UserResponse ToResponse(ApplicationUser user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email ?? string.Empty,
        user.IdNumber,
        user.IdType,
        user.Phone,
        user.PhoneExtra,
        user.Location,
        user.Address,
        user.PhotoUrl,
        user.CreatedAt);
}