using BackEndPets.Application.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly ILogger<FirebasePushNotificationSender> _logger;
    private readonly FirebaseApp? _app;

    public FirebasePushNotificationSender(
        IConfiguration configuration,
        ILogger<FirebasePushNotificationSender> logger)
    {
        _logger = logger;

        var credential = ResolveCredential(configuration);
        _app = credential is null
            ? null
            : FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions { Credential = credential });
    }

    private static GoogleCredential? ResolveCredential(IConfiguration configuration)
    {
        // Prefer the raw JSON (set via env var in deployment — a file path isn't
        // practical to provision on a remote host through a simple `docker run`),
        // falling back to a file path for local development.
        var credentialsJson = configuration["Firebase:CredentialsJson"];
        if (!string.IsNullOrWhiteSpace(credentialsJson))
        {
            return GoogleCredential.FromJson(credentialsJson);
        }

        var credentialsPath = configuration["Firebase:CredentialsPath"];
        if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
        {
            return GoogleCredential.FromFile(credentialsPath);
        }

        return null;
    }

    public bool IsConfigured => _app is not null;

    public async Task<IReadOnlyCollection<string>> SendAsync(
        IReadOnlyCollection<string> tokens,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct)
    {
        if (!IsConfigured || tokens.Count == 0) return [];

        var message = new MulticastMessage
        {
            Tokens = tokens.ToList(),
            Notification = new Notification { Title = title, Body = body },
            Data = data is null ? new Dictionary<string, string>() : new Dictionary<string, string>(data)
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, ct);
            if (response.FailureCount == 0) return [];

            var invalidTokens = new List<string>();
            var tokenList = tokens.ToList();
            for (var i = 0; i < response.Responses.Count; i++)
            {
                var r = response.Responses[i];
                if (r.IsSuccess) continue;
                if (r.Exception?.MessagingErrorCode is MessagingErrorCode.Unregistered
                    or MessagingErrorCode.InvalidArgument)
                {
                    invalidTokens.Add(tokenList[i]);
                }
            }
            return invalidTokens;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send push notification: {ExceptionType}", ex.GetType().Name);
            return [];
        }
    }
}
