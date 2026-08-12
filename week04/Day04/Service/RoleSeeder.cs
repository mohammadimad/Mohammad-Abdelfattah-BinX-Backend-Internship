using Microsoft.AspNetCore.Identity;

namespace Day03.Service
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@library.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "AdminP@ss123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var userEmail = "mohammad@library.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            if (normalUser == null)
            {
                normalUser = new IdentityUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
                await userManager.CreateAsync(normalUser, "UserP@ss123!");
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }
    }
}
