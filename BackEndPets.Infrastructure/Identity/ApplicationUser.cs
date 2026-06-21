using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
