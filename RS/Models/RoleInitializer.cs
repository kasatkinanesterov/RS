using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public static class RoleInitializer
{
    public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
    {
        // Получаем сервисы для работы с ролями и пользователями
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Определяем роли, которые нужно создать
        string[] roleNames = { "Admin", "Agent", "Other", "Designer", "Undertaker", "Organizer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Ошибка при создании роли: {roleName}");
                }
            }
        }

        // Данные для администратора
        string adminEmail = "admin@example.com";
        string adminPassword = "Admin123!";

        // Проверяем, существует ли уже пользователь с данным email
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            // Создаем администратора, если он не найден
            var newAdmin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Устанавливаем подтверждение email
            };

            var createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);
            if (!createAdminResult.Succeeded)
            {
                throw new Exception($"Ошибка при создании администратора: {string.Join(", ", createAdminResult.Errors.Select(e => e.Description))}");
            }

            // Добавляем пользователя в роль "Admin"
            var addToRoleResult = await userManager.AddToRoleAsync(newAdmin, "Admin");
            if (!addToRoleResult.Succeeded)
            {
                throw new Exception($"Ошибка при добавлении пользователя в роль Admin: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
