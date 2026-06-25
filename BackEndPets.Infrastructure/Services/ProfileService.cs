using BackEndPets.Application.DTOs.Profiles;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BackEndPets.Infrastructure.Services;

public sealed class ProfileService(
    UserManager<ApplicationUser> userManager,
    IWalkerRepository walkerRepository,
    IBusinessRepository businessRepository) : IProfileService
{
    private static readonly string[] ValidBusinessTypes = ["veterinary", "grooming", "shelter", "petshop", "other"];

    public async Task<MeResponse?> GetMeAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var walker = await walkerRepository.GetByUserIdAsync(userId);
        var businesses = await businessRepository.GetByOwnerIdAsync(userId);

        return new MeResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            IsWalker: walker is not null,
            WalkerStatus: walker?.VerificationStatus,
            Businesses: businesses
                .Select(b => new BusinessSummary(b.Id, b.Name, b.Type, b.VerificationStatus))
                .ToList());
    }

    public async Task<(WalkerResponse? Walker, string? ErrorCode)> BecomeWalkerAsync(Guid userId)
    {
        if (await walkerRepository.ExistsForUserAsync(userId))
            return (null, "WALKER_ALREADY_EXISTS");

        var created = await walkerRepository.CreateAsync(new Walker { UserId = userId });

        return (new WalkerResponse(
            created.Id,
            created.UserId,
            created.VerificationStatus,
            created.Available,
            created.WorkLocation,
            created.Experience,
            created.Description,
            created.CreatedAt), null);
    }

    public async Task<(BusinessResponse? Business, string? ErrorCode)> CreateBusinessAsync(
        Guid userId, CreateBusinessRequest request)
    {
        if (request.Type is not null && !ValidBusinessTypes.Contains(request.Type))
            return (null, "INVALID_BUSINESS_TYPE");

        var business = new Business
        {
            OwnerUserId = userId,
            Name = request.Name.Trim(),
            Nit = request.Nit.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone,
            Type = request.Type?.Trim(),
            Location = request.Location?.Trim(),
            Address = request.Address?.Trim()
        };

        try
        {
            var created = await businessRepository.CreateAsync(business);
            return (new BusinessResponse(
                created.Id,
                created.OwnerUserId,
                created.Name,
                created.Nit,
                created.Email,
                created.Phone,
                created.Type,
                created.Location,
                created.Address,
                created.VerificationStatus,
                created.CreatedAt), null);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" } pg)
        {
            var errorCode = pg.ConstraintName?.Contains("nit") == true
                ? "NIT_ALREADY_TAKEN"
                : "EMAIL_ALREADY_TAKEN";
            return (null, errorCode);
        }
    }
}
