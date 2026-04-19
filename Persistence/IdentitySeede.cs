using Backend.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Backend.Persistence
{
    public static class IdentitySeed
    {
        public static async Task SeedRolesAndUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            AppDbContext context)
        {
            Console.WriteLine("🚀 Starting Identity seeding...");

            // 1️⃣ Define roles and create them if not exists
            var roles = new[] { "Administrator", "User" };
            await CreateRolesAsync(roleManager, roles);

            await context.SaveChangesAsync();

            // 2️⃣ Create Administrator user
            var superEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var superPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(superEmail) || string.IsNullOrWhiteSpace(superPassword))
            {
                Console.WriteLine("AppCredentials not configured for Administrator.");
            }
            else
            {
                // Create the Administrator user
                await CreateUserIfNotExistsAsync(userManager, superEmail, superPassword, "Administrator", "System", "Administrator");
                // Assign Administrator role
                var adminUser = await userManager.FindByEmailAsync(superEmail);
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }

            // 3️⃣ Create a regular user (User role)
            var userEmail = Environment.GetEnvironmentVariable("USER_EMAIL");
            var userPassword = Environment.GetEnvironmentVariable("USER_PASSWORD");

            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userPassword))
            {
                Console.WriteLine("AppCredentials not configured for User.");
            }
            else
            {
                // Create the regular User
                await CreateUserIfNotExistsAsync(userManager, userEmail, userPassword, "Regular", "User", "User");
                // Assign User role
                var regularUser = await userManager.FindByEmailAsync(userEmail);
                await userManager.AddToRoleAsync(regularUser, "User");
            }

            // 4️⃣ Save all changes
            await context.SaveChangesAsync();

            Console.WriteLine("Identity seeding completed successfully.");
        }

        private static async Task CreateRolesAsync(RoleManager<ApplicationRole> roleManager, string[] roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    Console.WriteLine(result.Succeeded
                        ? $"Role '{roleName}' created."
                        : $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async Task CreateUserIfNotExistsAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string roleName,
            string firstName,
            string lastName,
            string address = "Somalia Street")
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    Address = address
                };

                var createResult = await userManager.CreateAsync(newUser, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, roleName); // Assign the role
                    Console.WriteLine($"{roleName} user created and assigned to '{roleName}' role.");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create {roleName} user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"{roleName} user already exists.");
            }
        }
    }
}