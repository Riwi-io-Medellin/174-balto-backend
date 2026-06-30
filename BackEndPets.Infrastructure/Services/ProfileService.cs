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
    IBusinessRepository businessRepository,
    IFeedbackRepository feedbackRepository) : IProfileService
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

        var user = await userManager.FindByIdAsync(userId.ToString());
        return (new WalkerResponse(
            created.Id,
            created.UserId,
            $"{user?.FirstName ?? ""} {user?.LastName ?? ""}".Trim(),
            user?.PhotoUrl,
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
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Nit) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            request.Phone <= 0)
            return (null, "VALIDATION_FAILED");

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
    
    public async Task<IReadOnlyCollection<WalkerResponse>> GetWalkersAsync(bool? available = null, string? workLocation = null)
    {
        var projections = await walkerRepository.GetAllWithUserAsync(available, workLocation);
        return projections.Select(p => new WalkerResponse(
            p.Walker.Id, p.Walker.UserId,
            $"{p.FirstName} {p.LastName}", p.PhotoUrl,
            p.Walker.VerificationStatus, p.Walker.Available,
            p.Walker.WorkLocation, p.Walker.Experience,
            p.Walker.Description, p.Walker.CreatedAt))
            .ToList();
    }

    public async Task<WalkerResponse?> GetWalkerByIdAsync(Guid id)
    {
        var projection = await walkerRepository.GetByIdWithUserAsync(id);
        if (projection is null) return null;
        return new WalkerResponse(
            projection.Walker.Id, projection.Walker.UserId,
            $"{projection.FirstName} {projection.LastName}", projection.PhotoUrl,
            projection.Walker.VerificationStatus, projection.Walker.Available,
            projection.Walker.WorkLocation, projection.Walker.Experience,
            projection.Walker.Description, projection.Walker.CreatedAt);
    }

    public async Task<IReadOnlyCollection<BusinessResponse>> GetBusinessesAsync(string? type = null, string? location = null) =>
        (await businessRepository.GetAllAsync(type, location))
        .Select(b => new BusinessResponse(b.Id, b.OwnerUserId, b.Name, b.Nit,
            b.Email, b.Phone, b.Type, b.Location, b.Address, b.VerificationStatus, b.CreatedAt))
        .ToList();

    public async Task<BusinessResponse?> GetBusinessByIdAsync(Guid id)
    {
        var b = await businessRepository.GetByIdAsync(id);
        return b is null ? null : new BusinessResponse(b.Id, b.OwnerUserId, b.Name, b.Nit,
            b.Email, b.Phone, b.Type, b.Location, b.Address, b.VerificationStatus, b.CreatedAt);
    }
    
    public async Task<(WalkerProfileResponse? Profile, string? ErrorCode)> GetMyWalkerProfileAsync(Guid userId)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        return walker is null
            ? (null, "WALKER_NOT_FOUND")
            : (MapWalkerProfile(walker), null);
    }

    public async Task<(WalkerProfileResponse? Profile, string? ErrorCode)> UpdateMyWalkerProfileAsync(
        Guid userId, UpdateWalkerProfileRequest request)
    {
        var walker = await walkerRepository.GetByUserIdAsync(userId);
        if (walker is null)
            return (null, "WALKER_NOT_FOUND");

        if (walker.VerificationStatus != "approved")
            return (null, "WALKER_NOT_APPROVED");

        if (request.HourlyRate.HasValue && request.HourlyRate.Value < 0)
            return (null, "HOURLY_RATE_INVALID");

        if (request.ServiceRadiusKm.HasValue && request.ServiceRadiusKm.Value <= 0)
            return (null, "SERVICE_RADIUS_INVALID");

        if (request.YearsOfExperience.HasValue && request.YearsOfExperience.Value < 0)
            return (null, "YEARS_OF_EXPERIENCE_INVALID");

        if (request.MaxDogs.HasValue && request.MaxDogs.Value < 1)
            return (null, "MAX_DOGS_INVALID");

        if (request.Bio is not null)
            walker.Bio = request.Bio.Trim();

        if (request.HourlyRate.HasValue)
            walker.HourlyRate = request.HourlyRate.Value;

        if (request.ServiceRadiusKm.HasValue)
            walker.ServiceRadiusKm = request.ServiceRadiusKm.Value;

        if (request.YearsOfExperience.HasValue)
            walker.YearsOfExperience = request.YearsOfExperience.Value;

        if (request.IsAcceptingBookings.HasValue)
            walker.IsAcceptingBookings = request.IsAcceptingBookings.Value;

        if (request.WorkLatitude.HasValue)
            walker.WorkLatitude = request.WorkLatitude.Value;

        if (request.WorkLongitude.HasValue)
            walker.WorkLongitude = request.WorkLongitude.Value;

        if (request.MaxDogs.HasValue)
            walker.MaxDogs = request.MaxDogs.Value;

        await walkerRepository.UpdateAsync(walker);
        return (MapWalkerProfile(walker), null);
    }

    private static WalkerProfileResponse MapWalkerProfile(Walker w) => new(
        w.Id, w.UserId, w.VerificationStatus,
        w.Available, w.WorkLocation, w.Experience, w.Description,
        w.Bio, w.HourlyRate, w.ServiceRadiusKm, w.YearsOfExperience, w.IsAcceptingBookings,
        w.DocumentName, w.DocumentNumber,
        w.WorkLatitude, w.WorkLongitude, w.MaxDogs,
        w.CreatedAt, w.UpdatedAt);

    public async Task<IReadOnlyCollection<WalkerRecommendationResponse>> GetWalkerRecommendationsAsync(
        WalkerRecommendationRequest request)
    {
        var walkers = await walkerRepository.GetAllAsync(available: true);
        var result = new List<(Walker Walker, int Score, List<string> Reasons)>();

        foreach (var w in walkers.Where(w => w.VerificationStatus == "approved"))
        {
            var reasons = new List<string>();
            var score = 0;

            if (w.Available)
            {
                reasons.Add("Available now");
                score += 3;
            }

            if (!string.IsNullOrWhiteSpace(request.WorkLocation) &&
                !string.IsNullOrWhiteSpace(w.WorkLocation) &&
                w.WorkLocation.ToLower().Contains(request.WorkLocation.ToLower()))
            {
                reasons.Add($"Operates in {request.WorkLocation}");
                score += 2;
            }

            if (!string.IsNullOrWhiteSpace(w.Experience))
            {
                reasons.Add("With documented experience");
                score += 1;
            }

            // Scoring por calificación promedio
            var feedbacks = await feedbackRepository.GetByTargetAsync(w.Id, "walker");
            if (feedbacks.Count > 0)
            {
                var avgRating = feedbacks.Average(f => f.Rating);
                if (avgRating >= 4.0)
                {
                    reasons.Add($"Average rating {Math.Round(avgRating, 1)}/5");
                    score += (int)Math.Round(avgRating);
                }
            }

            if (reasons.Count > 0)
                result.Add((w, score, reasons));
        }

        return result
            .OrderByDescending(x => x.Score)
            .Select(x => new WalkerRecommendationResponse(
                x.Walker.Id,
                x.Walker.UserId,
                x.Walker.Available,
                x.Walker.WorkLocation,
                x.Walker.Experience,
                x.Walker.Description,
                x.Walker.VerificationStatus,
                x.Reasons))
            .ToList();
    }
}
