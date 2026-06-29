using BackEndPets.Application.DTOs.Bookings;
using BackEndPets.Application.DTOs.Common;

namespace BackEndPets.Application.Interfaces;

public interface IWalkBookingService
{
    Task<(BookingResponse? Result, string? ErrorCode)> CreateAsync(
        Guid clientUserId, CreateBookingRequest request);

    Task<PagedResult<BookingResponse>> GetMyBookingsAsync(
        Guid clientUserId, string? status = null, int page = 1, int pageSize = 20);

    /// <summary>Returns the booking only if currentUserId is the client or the assigned walker's user.</summary>
    Task<(BookingResponse? Result, string? ErrorCode)> GetByIdAsync(
        Guid currentUserId, Guid bookingId);

    Task<PagedResult<BookingResponse>> GetPendingForWalkerAsync(
        Guid walkerUserId, int page = 1, int pageSize = 20);

    Task<PagedResult<BookingResponse>> GetWalkerBookingsAsync(
        Guid walkerUserId, string? status = null, int page = 1, int pageSize = 20);

    Task<(BookingResponse? Result, string? ErrorCode)> AcceptAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingResponse? Result, string? ErrorCode)> RejectAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingResponse? Result, string? ErrorCode)> CancelByWalkerAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingResponse? Result, string? ErrorCode)> CancelByOwnerAsync(
        Guid clientUserId, Guid bookingId);
}
