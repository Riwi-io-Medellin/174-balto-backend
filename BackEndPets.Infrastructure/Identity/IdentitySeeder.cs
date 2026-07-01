using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure.Identity;

public static class IdentitySeeder
{
    private const string DemoEmail = "admin@balto.io";
    private const string DemoPassword = "Password123!";
    private const string AdminRole = "Admin";

    public static async Task SeedDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(AdminRole));
        }

        var existingUser = await userManager.FindByEmailAsync(DemoEmail);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, AdminRole))
            {
                await userManager.AddToRoleAsync(existingUser, AdminRole);
            }

            return;
        }

        var user = new ApplicationUser
        {
            UserName = "Admin.PawExplorers",
            Email = DemoEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "PawExplorers",
            IdNumber = "0000000000",
            IdType = "CC",
            Phone = "3000000000"
        };

        var result = await userManager.CreateAsync(user, DemoPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, AdminRole);
        }
    }
}
