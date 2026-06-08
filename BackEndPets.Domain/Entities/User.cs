using BackEndPets.Domain.Common;

namespace BackEndPets.Domain.Entities;

public sealed class User : EntityBase
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;

    public string IdType { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? Address { get; set; }

    public long Phone { get; set; }

    public long? PhoneExtra { get; set; }

    public string? PhotoUrl { get; set; }
}
