using ETFTalentProgram.Constants;
using ETFTalentProgram.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Data
{
    public static class DbInitializer
    {
        private static readonly string[] Roles =
        [
            AppRoles.Student,
            AppRoles.Firma,
            AppRoles.Referent,
            AppRoles.Administrator
        ];

        private static readonly (string Email, string Password, string Role)[] SeedUsers =
        [
            ("admin@etf.ba", "Admin123!", AppRoles.Administrator),
            ("firma@etf.ba", "Firma123!", AppRoles.Firma),
            ("student@etf.ba", "Student123!", AppRoles.Student),
            ("referent@etf.ba", "Referent123!", AppRoles.Referent)
        ];

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role
            )
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
                return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DatumRegistracije = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            foreach (var (email, password, role) in SeedUsers)
            {
                await CreateUserIfNotExists(userManager, email, password, role);
            }
        }
    }
}
