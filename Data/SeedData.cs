using grad.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace grad.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            // Get RoleManager from DI
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // Default roles
            string[] roles = new[] { "Admin", "Student", "Teacher" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }


            Console.WriteLine("Roles seeded successfully!");
        }
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Check if admin user already exists
            var adminEmail = "m314227@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    firstname = "Graduation",
                    lastname = "Project",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin@123"); // set password
            }

            // Assign Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            Console.WriteLine("Admin user seeded successfully!");


        }
    }
}
