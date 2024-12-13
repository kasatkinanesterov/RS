using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RS.Models;

public static class RoleInitializer
{
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Employee>>();

        string[] roleNames = { "Admin", "Agent", "Other" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var newAdmin = new Employee
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "Administrator",
                Department = "Management"
            };
            var result = await userManager.CreateAsync(newAdmin, "Admin123Admin!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
}
