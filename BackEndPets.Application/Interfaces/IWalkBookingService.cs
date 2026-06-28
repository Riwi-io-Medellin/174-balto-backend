using BackEndPets.Application.DTOs.Bookings;

namespace BackEndPets.Application.Interfaces;

public interface IWalkBookingService
{
    Task<(BookingResponse? Result, string? ErrorCode)> CreateAsync(
        Guid clientUserId, CreateBookingRequest request);

    Task<IReadOnlyCollection<BookingResponse>> GetMyBookingsAsync(
        Guid clientUserId, string? status = null);

    /// <summary>Returns the booking only if currentUserId is the client or the assigned walker's user.</summary>
    Task<(BookingResponse? Result, string? ErrorCode)> GetByIdAsync(
        Guid currentUserId, Guid bookingId);

    Task<(IReadOnlyCollection<BookingResponse>? Result, string? ErrorCode)> GetPendingForWalkerAsync(
        Guid walkerUserId);

    Task<(IReadOnlyCollection<BookingResponse>? Result, string? ErrorCode)> GetWalkerBookingsAsync(
        Guid walkerUserId, string? status = null);

    Task<(BookingResponse? Result, string? ErrorCode)> AcceptAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingResponse? Result, string? ErrorCode)> RejectAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingResponse? Result, string? ErrorCode)> CancelByWalkerAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingResponse? Result, string? ErrorCode)> CancelByOwnerAsync(
        Guid clientUserId, Guid bookingId);
}
