using Microsoft.AspNetCore.Identity;
using SIS.Common.Constants;
using SIS.Entities.Identity;

namespace SIS.API.Extensions
{
    public static class SeedDataExtensions
    {
        public static async Task SeedDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();

                
                await SeedRolesAsync(roleManager);

                
                await SeedSuperAdminAsync(userManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Seed data xətası");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
            RoleConstants.SuperAdmin,
            RoleConstants.Admin,
            RoleConstants.Moderator,
            RoleConstants.Teacher,
            RoleConstants.Student
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<AppUser> userManager)
        {
            var superAdmin = await userManager.FindByEmailAsync("superadmin@sis.az");
            if (superAdmin != null) return;

            var user = new AppUser
            {
                FullName = "Super Admin",
                Email = "superadmin@sis.az",
                UserName = "superadmin@sis.az",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "SuperAdmin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, RoleConstants.SuperAdmin);
        }
    }
}
