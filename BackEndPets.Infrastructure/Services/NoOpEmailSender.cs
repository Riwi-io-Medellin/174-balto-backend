using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;

public sealed class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody)
    {
        logger.LogWarning(
            "[NoOpEmailSender] Email NOT sent — no provider configured.\n" +
            "  To: {To}\n  Subject: {Subject}\n  Body: {Body}",
            to, subject, htmlBody);

        return Task.CompletedTask;
    }
}
