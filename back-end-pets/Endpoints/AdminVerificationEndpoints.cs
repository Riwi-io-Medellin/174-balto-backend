using BackEndPets.Application.DTOs.Admin;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

public static class AdminVerificationEndpoints
{
    public static IEndpointRouteBuilder MapAdminVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/verification")
            .WithTags("Admin Verification")
            .RequireAuthorization();

        group.MapGet("/businesses", async (string? status, IAdminVerificationService service) =>
                Results.Ok(await service.GetBusinessesAsync(status)))
            .WithName("GetAdminBusinessVerifications")
            .WithSummary("List businesses with verification documents")
            .Produces<IReadOnlyCollection<AdminBusinessVerificationResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/businesses/{businessId:guid}/status", async (
            Guid businessId,
            UpdateVerificationStatusRequest request,
            IAdminVerificationService service) =>
        {
            var business = await service.UpdateBusinessStatusAsync(businessId, request.VerificationStatus);
            return business is null
                ? Results.BadRequest(new ApiErrorResponse(
                    "Business not found or invalid status. Allowed: pending, approved, rejected.",
                    "BUSINESS_STATUS_UPDATE_FAILED"))
                : Results.Ok(business);
        })
        .WithName("UpdateAdminBusinessVerificationStatus")
        .WithSummary("Update business verification status")
        .Produces<AdminBusinessVerificationResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/walkers", async (string? status, IAdminVerificationService service) =>
                Results.Ok(await service.GetWalkersAsync(status)))
            .WithName("GetAdminWalkerVerifications")
            .WithSummary("List walkers with verification documents")
            .Produces<IReadOnlyCollection<AdminWalkerVerificationResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/walkers/{walkerId:guid}/status", async (
            Guid walkerId,
            UpdateVerificationStatusRequest request,
            IAdminVerificationService service) =>
        {
            var walker = await service.UpdateWalkerStatusAsync(walkerId, request.VerificationStatus);
            return walker is null
                ? Results.BadRequest(new ApiErrorResponse(
                    "Walker not found or invalid status. Allowed: pending, approved, rejected.",
                    "WALKER_STATUS_UPDATE_FAILED"))
                : Results.Ok(walker);
        })
        .WithName("UpdateAdminWalkerVerificationStatus")
        .WithSummary("Update walker verification status")
        .Produces<AdminWalkerVerificationResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

        return app;
    }
}
