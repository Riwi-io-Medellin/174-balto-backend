using BackEndPets.Application.DTOs.WalkSessions;

namespace BackEndPets.Application.Interfaces;

public interface IWalkSessionService
{
    Task<(WalkSessionResponse? Session, string? ErrorCode)> StartSessionAsync(
        Guid currentUserId, StartWalkSessionRequest request);

    Task<(WalkRoutePointResponse? Point, string? ErrorCode)> AddLocationAsync(
        Guid currentUserId, Guid sessionId, AddLocationRequest request);

    Task<(WalkSessionResponse? Session, string? ErrorCode)> PauseSessionAsync(
        Guid currentUserId, Guid sessionId);

    Task<(WalkSessionResponse? Session, string? ErrorCode)> ResumeSessionAsync(
        Guid currentUserId, Guid sessionId);

    Task<(WalkSessionResponse? Session, string? ErrorCode)> CompleteSessionAsync(
        Guid currentUserId, Guid sessionId);

    Task<(IReadOnlyCollection<WalkRoutePointResponse>? Points, string? ErrorCode)> GetRouteAsync(
        Guid currentUserId, Guid sessionId);

    // ── Booking-based lifecycle ───────────────────────────────────────────────

    Task<(BookingSessionResponse? Session, string? ErrorCode)> StartFromBookingAsync(
        Guid walkerUserId, Guid bookingId);

    Task<(BookingSessionResponse? Session, string? ErrorCode)> FinishAsync(
        Guid walkerUserId, Guid sessionId, FinishSessionRequest request);

    Task<(BookingSessionResponse? Session, string? ErrorCode)> GetSessionDetailAsync(
        Guid sessionId);

    Task<(BookingSessionResponse? Session, string? ErrorCode)> GetActiveForWalkerAsync(
        Guid walkerUserId);

    // ── Walk session media ────────────────────────────────────────────────────

    Task<(WalkSessionMediaResponse? Media, string? ErrorCode)> AddMediaAsync(
        Guid walkerUserId, Guid sessionId, AddWalkMediaRequest request);

    Task<(IReadOnlyCollection<WalkSessionMediaResponse>? Media, string? ErrorCode)> GetMediaAsync(
        Guid currentUserId, Guid sessionId);

    // ── Walk chat ──────────────────────────────────────────────────────────────

    Task<(ChatMessageResponse? Message, string? ErrorCode)> SendChatMessageAsync(
        Guid currentUserId, Guid sessionId, SendChatMessageRequest request);

    Task<(IReadOnlyCollection<ChatMessageResponse>? Messages, string? ErrorCode)> GetChatMessagesAsync(
        Guid currentUserId, Guid sessionId);
}
