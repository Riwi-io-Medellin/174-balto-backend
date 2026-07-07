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
    IBusinessServiceRepository businessServiceRepository,
    IBusinessHourRepository businessHourRepository,
    IBusinessHourExceptionRepository businessHourExceptionRepository,
    IFeedbackRepository feedbackRepository) : IProfileService
{
    private static readonly string[] ValidBusinessTypes = ["veterinary", "grooming", "shelter", "petshop", "other"];

    /// Computes whether a business is open right now from its weekly hours + today's exception (Bogotá time, UTC-5).
    private async Task<bool> IsOpenNowAsync(Guid businessId)
    {
        var now = DateTime.UtcNow.AddHours(-5);
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var exceptions = await businessHourExceptionRepository.GetByBusinessIdAsync(businessId);
        var todayException = exceptions.FirstOrDefault(e => e.Date == today);
        if (todayException is not null)
        {
            if (todayException.IsUnavailable) return false;
            return todayException.StartTime is not null && todayException.EndTime is not null &&
                   nowTime >= todayException.StartTime && nowTime <= todayException.EndTime;
        }

        var hours = await businessHourRepository.GetByBusinessIdAsync(businessId);
        var todayHour = hours.FirstOrDefault(h => h.DayOfWeek == (int)now.DayOfWeek && h.IsActive);
        return todayHour is not null && nowTime >= todayHour.StartTime && nowTime <= todayHour.EndTime;
    }

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
            created.CreatedAt,
            created.InstagramUrl,
            created.FacebookUrl), null);
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
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude
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
                created.CreatedAt,
                created.InstagramUrl,
                created.FacebookUrl,
                created.Description,
                created.PhotoUrl,
                Latitude: created.Latitude,
                Longitude: created.Longitude,
                SellsServices: created.SellsServices,
                SellsProducts: created.SellsProducts), null);
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
        var walkerIds = projections.Select(p => p.Walker.Id).ToList();
        var feedbacks = await feedbackRepository.GetByTargetsAsync(walkerIds, "walker");
        var ratingMap = feedbacks
            .GroupBy(f => f.TargetId)
            .ToDictionary(g => g.Key, g => (Average: g.Average(f => f.Rating), Count: g.Count()));

        return projections.Select(p =>
        {
            var (avg, count) = ratingMap.GetValueOrDefault(p.Walker.Id, (0.0, 0));
            return new WalkerResponse(
                p.Walker.Id, p.Walker.UserId,
                $"{p.FirstName} {p.LastName}", p.PhotoUrl,
                p.Walker.VerificationStatus, p.Walker.Available,
                p.Walker.WorkLocation, p.Walker.Experience,
                p.Walker.Description, p.Walker.CreatedAt,
                p.Walker.InstagramUrl, p.Walker.FacebookUrl,
                Math.Round(avg, 1), count);
        }).ToList();
    }

    public async Task<WalkerResponse?> GetWalkerByIdAsync(Guid id)
    {
        var projection = await walkerRepository.GetByIdWithUserAsync(id);
        if (projection is null) return null;

        var feedbacks = await feedbackRepository.GetByTargetAsync(id, "walker");
        var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;

        return new WalkerResponse(
            projection.Walker.Id, projection.Walker.UserId,
            $"{projection.FirstName} {projection.LastName}", projection.PhotoUrl,
            projection.Walker.VerificationStatus, projection.Walker.Available,
            projection.Walker.WorkLocation, projection.Walker.Experience,
            projection.Walker.Description, projection.Walker.CreatedAt,
            projection.Walker.InstagramUrl, projection.Walker.FacebookUrl,
            Math.Round(avg, 1), feedbacks.Count);
    }

    public async Task<IReadOnlyCollection<BusinessResponse>> GetBusinessesAsync(string? type = null, string? location = null)
    {
        var businesses = await businessRepository.GetAllAsync(type, location);
        var businessIds = businesses.Select(b => b.Id).ToList();
        var feedbacks = await feedbackRepository.GetByTargetsAsync(businessIds, "business");
        var ratingMap = feedbacks
            .GroupBy(f => f.TargetId)
            .ToDictionary(g => g.Key, g => (Average: g.Average(f => f.Rating), Count: g.Count()));
    
        var result = new List<BusinessResponse>();
        foreach (var b in businesses)
        {
            var (avg, count) = ratingMap.GetValueOrDefault(b.Id, (0.0, 0));
            var isOpen = await IsOpenNowAsync(b.Id);
            result.Add(new BusinessResponse(b.Id, b.OwnerUserId, b.Name, b.Nit,
                b.Email, b.Phone, b.Type, b.Location, b.Address, b.VerificationStatus,
                b.CreatedAt, b.InstagramUrl, b.FacebookUrl,
                b.Description, b.PhotoUrl,
                Math.Round(avg, 1), count,
                Latitude: b.Latitude, Longitude: b.Longitude, IsOpenNow: isOpen,
                SellsServices: b.SellsServices, SellsProducts: b.SellsProducts));
        }
        return result;
    }
    
    public async Task<BusinessResponse?> GetBusinessByIdAsync(Guid id)
    {
        var b = await businessRepository.GetByIdAsync(id);
        if (b is null) return null;
    
        var feedbacks = await feedbackRepository.GetByTargetAsync(b.Id, "business");
        var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;
        var isOpen = await IsOpenNowAsync(b.Id);
    
        return new BusinessResponse(b.Id, b.OwnerUserId, b.Name, b.Nit,
            b.Email, b.Phone, b.Type, b.Location, b.Address, b.VerificationStatus,
            b.CreatedAt, b.InstagramUrl, b.FacebookUrl,
            b.Description, b.PhotoUrl,
            Math.Round(avg, 1), feedbacks.Count,
            Latitude: b.Latitude, Longitude: b.Longitude, IsOpenNow: isOpen,
            SellsServices: b.SellsServices, SellsProducts: b.SellsProducts);
    }
    
    public async Task<BusinessResponse?> GetMyBusinessAsync(Guid userId)
    {
        var business = (await businessRepository.GetByOwnerIdAsync(userId)).FirstOrDefault();
        return business is null ? null : await GetBusinessByIdAsync(business.Id);
    }

    public async Task<(BusinessResponse? Business, string? ErrorCode)> UpdateMyBusinessAsync(
        Guid userId, UpdateBusinessRequest request)
    {
        var business = (await businessRepository.GetByOwnerIdAsync(userId)).FirstOrDefault();
        if (business is null)
            return (null, "BUSINESS_NOT_FOUND");
    
        if (business.VerificationStatus != "approved")
            return (null, "BUSINESS_NOT_APPROVED");
    
        if (request.InstagramUrl is not null)
            business.InstagramUrl = request.InstagramUrl.Trim();
    
        if (request.FacebookUrl is not null)
            business.FacebookUrl = request.FacebookUrl.Trim();

        if (request.Description is not null)
            business.Description = request.Description.Trim();

        if (request.PhotoUrl is not null)
            business.PhotoUrl = request.PhotoUrl.Trim();

        if (request.SellsServices is not null)
            business.SellsServices = request.SellsServices.Value;

        if (request.SellsProducts is not null)
            business.SellsProducts = request.SellsProducts.Value;
    
        await businessRepository.UpdateAsync(business);
    
        var feedbacks = await feedbackRepository.GetByTargetAsync(business.Id, "business");
        var avg = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0.0;
        var isOpen = await IsOpenNowAsync(business.Id);
    
        return (new BusinessResponse(
            business.Id, business.OwnerUserId, business.Name, business.Nit,
            business.Email, business.Phone, business.Type, business.Location,
            business.Address, business.VerificationStatus,
            business.CreatedAt, business.InstagramUrl, business.FacebookUrl,
            business.Description, business.PhotoUrl,
            Math.Round(avg, 1), feedbacks.Count,
            Latitude: business.Latitude, Longitude: business.Longitude, IsOpenNow: isOpen,
            SellsServices: business.SellsServices, SellsProducts: business.SellsProducts), null);
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

        if (request.InstagramUrl is not null)
            walker.InstagramUrl = request.InstagramUrl.Trim();

        if (request.FacebookUrl is not null)
            walker.FacebookUrl = request.FacebookUrl.Trim();

        await walkerRepository.UpdateAsync(walker);
        return (MapWalkerProfile(walker), null);
    }

    private static WalkerProfileResponse MapWalkerProfile(Walker w) => new(
        w.Id, w.UserId, w.VerificationStatus,
        w.Available, w.WorkLocation, w.Experience, w.Description,
        w.Bio, w.HourlyRate, w.ServiceRadiusKm, w.YearsOfExperience, w.IsAcceptingBookings,
        w.DocumentName, w.DocumentNumber,
        w.WorkLatitude, w.WorkLongitude, w.MaxDogs,
        w.CreatedAt, w.UpdatedAt,
        w.InstagramUrl, w.FacebookUrl);

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