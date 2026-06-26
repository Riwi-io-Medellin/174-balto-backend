using System.Net;
using System.Net.Mail;
using BackEndPets.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndPets.Infrastructure.Services;
public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var host     = configuration["Smtp:Host"]     ?? "smtp.gmail.com";
        var port     = int.Parse(configuration["Smtp:Port"] ?? "587");
        var user     = configuration["Smtp:User"]     ?? throw new InvalidOperationException("Smtp:User not configured.");
        var password = configuration["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password not configured.");
        var from     = configuration["Smtp:From"]     ?? user;

        using var client = new SmtpClient(host, port)
        {
            Credentials  = new NetworkCredential(user, password),
            EnableSsl    = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var message = new MailMessage
        {
            From       = new MailAddress(from, "PawExplorers"),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message);
            logger.LogInformation("[SmtpEmailSender] Email sent to {To} — Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SmtpEmailSender] Failed to send email to {To}", to);
            throw;
        }
    }
}
