using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeServiceBookingService
{
    Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> CreateAsync(
        Guid clientUserId, CreateHomeServiceBookingRequest request);

    Task<PagedResult<HomeServiceBookingResponse>> GetMyBookingsAsync(
        Guid clientUserId, string? status = null, int page = 1, int pageSize = 20);

    /// <summary>Returns the booking only if currentUserId is the client or the assigned provider's user.</summary>
    Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> GetByIdAsync(
        Guid currentUserId, Guid bookingId);

    Task<PagedResult<HomeServiceBookingResponse>> GetPendingForProviderAsync(
        Guid providerUserId, int page = 1, int pageSize = 20);

    Task<PagedResult<HomeServiceBookingResponse>> GetProviderBookingsAsync(
        Guid providerUserId, string? status = null, int page = 1, int pageSize = 20);

    Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> AcceptAsync(
        Guid providerUserId, Guid bookingId);

    Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> RejectAsync(
        Guid providerUserId, Guid bookingId);

    Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> CancelByProviderAsync(
        Guid providerUserId, Guid bookingId);

    Task<(HomeServiceBookingResponse? Result, string? ErrorCode)> CancelByClientAsync(
        Guid clientUserId, Guid bookingId);
}
