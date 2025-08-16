using Microsoft.AspNetCore.Identity;

namespace TestApp.utilities;

public class SeedRoles
{
    public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    
}