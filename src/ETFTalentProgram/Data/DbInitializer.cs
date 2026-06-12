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

            const string adminEmail = "admin@etf.ba";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DatumRegistracije = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
                }
            }

            const string firmaEmail = "firma@etf.ba";
            if (await userManager.FindByEmailAsync(firmaEmail) == null)
            {
                var firma = new ApplicationUser
                {
                    UserName = firmaEmail,
                    Email = firmaEmail,
                    EmailConfirmed = true,
                    DatumRegistracije = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(firma, "Firma123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(firma, AppRoles.Firma);
                }
            }

            const string studentEmail = "student@etf.ba";
            if (await userManager.FindByEmailAsync(studentEmail) == null)
            {
                var student = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true,
                    DatumRegistracije = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(student, "Student123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, AppRoles.Student);
                }
            }

            const string referentEmail = "referent@etf.ba";
            if (await userManager.FindByEmailAsync(referentEmail) == null)
            {
                var referent = new ApplicationUser
                {
                    UserName = referentEmail,
                    Email = referentEmail,
                    EmailConfirmed = true,
                    DatumRegistracije = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(referent, "Referent123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(referent, AppRoles.Referent);
                }
            }
        }
    }
}
