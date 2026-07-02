using BackEndPets.Application.DTOs.Admin;
using BackEndPets.Application.DTOs.Notifications;
using BackEndPets.Application.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Services;

public sealed class AdminVerificationService(
    AppIdentityDbContext dbContext,
    INotificationService notificationService) : IAdminVerificationService
{
    private static readonly HashSet<string> AllowedStatuses = ["pending", "approved", "rejected"];

    public async Task<IReadOnlyCollection<AdminBusinessVerificationResponse>> GetBusinessesAsync(string? status = null)
    {
        var query = dbContext.Businesses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(b => b.VerificationStatus == status.Trim().ToLowerInvariant());
        }

        var businesses = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        var businessIds = businesses.Select(b => b.Id).ToList();
        var documents = await dbContext.BusinessDocuments
            .AsNoTracking()
            .Where(d => businessIds.Contains(d.BusinessId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return businesses
            .Select(b => new AdminBusinessVerificationResponse(
                b.Id,
                b.OwnerUserId,
                b.Name,
                b.Nit,
                b.Email,
                b.Phone,
                b.Type,
                b.Location,
                b.Address,
                b.VerificationStatus,
                b.CreatedAt,
                documents
                    .Where(d => d.BusinessId == b.Id)
                    .Select(d => new AdminBusinessDocumentResponse(d.Id, d.DocumentType, d.FileUrl, d.CreatedAt))
                    .ToList()))
            .ToList();
    }

    public async Task<AdminBusinessVerificationResponse?> UpdateBusinessStatusAsync(
        Guid businessId,
        string verificationStatus)
    {
        var status = NormalizeStatus(verificationStatus);
        if (status is null) return null;

        var business = await dbContext.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);
        if (business is null) return null;

        business.VerificationStatus = status;
        await dbContext.SaveChangesAsync();

        if (status is "approved" or "rejected")
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: business.OwnerUserId,
                Type: status == "approved" ? "business_approved" : "business_rejected",
                Title: status == "approved" ? "Business Approved" : "Business Rejected",
                Body: status == "approved"
                    ? "Your business has been verified and approved."
                    : "Your business was not approved. Please review the documents you submitted.",
                EntityId: business.Id,
                EntityType: "business"));
        }

        return (await GetBusinessesAsync()).FirstOrDefault(b => b.Id == businessId);
    }

    public async Task<IReadOnlyCollection<AdminWalkerVerificationResponse>> GetWalkersAsync(string? status = null)
    {
        var query = dbContext.Walkers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(w => w.VerificationStatus == status.Trim().ToLowerInvariant());
        }

        var walkers = await query
            .Join(dbContext.Users.AsNoTracking(),
                w => w.UserId,
                u => u.Id,
                (w, u) => new { Walker = w, User = u })
            .OrderByDescending(x => x.Walker.CreatedAt)
            .ToListAsync();

        var walkerIds = walkers.Select(x => x.Walker.Id).ToList();
        var documents = await dbContext.WalkerDocuments
            .AsNoTracking()
            .Where(d => walkerIds.Contains(d.WalkerId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return walkers
            .Select(x => new AdminWalkerVerificationResponse(
                x.Walker.Id,
                x.Walker.UserId,
                $"{x.User.FirstName} {x.User.LastName}".Trim(),
                x.User.PhotoUrl,
                x.Walker.Available,
                x.Walker.WorkLocation,
                x.Walker.Experience,
                x.Walker.Description,
                x.Walker.VerificationStatus,
                x.Walker.DocumentName,
                x.Walker.DocumentNumber,
                x.Walker.IsAcceptingBookings,
                x.Walker.CreatedAt,
                x.Walker.UpdatedAt,
                documents
                    .Where(d => d.WalkerId == x.Walker.Id)
                    .Select(d => new AdminWalkerDocumentResponse(d.Id, d.DocumentType, d.FileUrl, d.CreatedAt))
                    .ToList()))
            .ToList();
    }

    public async Task<AdminWalkerVerificationResponse?> UpdateWalkerStatusAsync(Guid walkerId, string verificationStatus)
    {
        var status = NormalizeStatus(verificationStatus);
        if (status is null) return null;

        var walker = await dbContext.Walkers.FirstOrDefaultAsync(w => w.Id == walkerId);
        if (walker is null) return null;

        walker.VerificationStatus = status;
        walker.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        if (status is "approved" or "rejected")
        {
            await notificationService.CreateAsync(new CreateNotificationRequest(
                UserId: walker.UserId,
                Type: status == "approved" ? "walker_approved" : "walker_rejected",
                Title: status == "approved" ? "Walker Approved" : "Walker Rejected",
                Body: status == "approved"
                    ? "You have been verified and approved as a walker."
                    : "Your walker verification was not approved. Please review the documents you submitted.",
                EntityId: walker.Id,
                EntityType: "walker"));
        }

        return (await GetWalkersAsync()).FirstOrDefault(w => w.Id == walkerId);
    }

    private static string? NormalizeStatus(string verificationStatus)
    {
        var status = verificationStatus.Trim().ToLowerInvariant();
        return AllowedStatuses.Contains(status) ? status : null;
    }
}