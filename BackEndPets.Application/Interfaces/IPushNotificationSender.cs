namespace BackEndPets.Application.Interfaces;

public interface IPushNotificationSender
{
    bool IsConfigured { get; }

    /// <summary>
    /// Sends a push notification to the given device tokens. Returns the subset of
    /// tokens that FCM reported as invalid/unregistered, so callers can prune them.
    /// </summary>
    Task<IReadOnlyCollection<string>> SendAsync(
        IReadOnlyCollection<string> tokens,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct);
}
