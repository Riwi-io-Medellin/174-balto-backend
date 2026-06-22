using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;
    
    public string IdType { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? Address { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? PhoneExtra { get; set; }

    public string? PhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
