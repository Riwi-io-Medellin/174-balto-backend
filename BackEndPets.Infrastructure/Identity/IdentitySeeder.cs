using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Identity;

public static class IdentitySeeder
{
    private const string DemoEmail = "admin@pawexplorers.com";
    private const string DemoPassword = "Password123!";

    public static async Task SeedDemoUserAsync(UserManager<ApplicationUser> userManager)
    {
        var existingUser = await userManager.FindByEmailAsync(DemoEmail);
        if (existingUser is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = DemoEmail,
            Email = DemoEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "PawExplorers",
            IdNumber = "0000000000",
            IdType = "CC",
            Phone = 3000000000
        };

        await userManager.CreateAsync(user, DemoPassword);
    }
}