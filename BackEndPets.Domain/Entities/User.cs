using BackEndPets.Domain.Common;

namespace BackEndPets.Domain.Entities;

public sealed class User : EntityBase
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
