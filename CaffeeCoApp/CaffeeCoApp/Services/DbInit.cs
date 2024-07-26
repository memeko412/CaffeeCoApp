using CaffeeCoApp.Models;
using Microsoft.AspNetCore.Identity;

namespace CaffeeCoApp.Services
{
    public class DbInit
    {
        public static async Task SeedDataAsync(UserManager<AppUser>? userManager, RoleManager<IdentityRole>? roleManager)
        { 
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("UserManager or RoleManager is null");
                return;
            }

            var adminExist = await roleManager.RoleExistsAsync("admin");
            if (!adminExist)
            { 
                Console.WriteLine("admin not initialized, creating admin role");
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            var staffExist = await roleManager.RoleExistsAsync("staff");
            if (!staffExist)
            {
                Console.WriteLine("staff not initialized, creating staff role");
                await roleManager.CreateAsync(new IdentityRole("staff"));
            }

            var clientExist = await roleManager.RoleExistsAsync("client");
            if (!clientExist)
            {
                Console.WriteLine("client not initialized, creating client role");
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            // create default admin if non-exist
            var adminUsers = userManager.GetUsersInRoleAsync("admin").Result;
            if(adminUsers.Any())
            {
                Console.WriteLine("Admin exist. Exiting.");
                return;
            }

            var user = new AppUser()
            {
                FirstName = "Default",
                LastName = "Admin",
                UserName = "DefaultAdmin",
                Email = "admin@caffeeco.com",
                CreatedAt = DateTime.Now,
            };

            string defaultAdminPassword = "AdminPassword@123";

            var result = await userManager.CreateAsync(user, defaultAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "admin");
                Console.WriteLine("Default admin created");
                Console.WriteLine("Username: " + user.UserName);
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Password: " + defaultAdminPassword);
                Console.WriteLine("Please update default password.");
            }
            else { 
                Console.WriteLine("Failed to create default admin");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }
    }
}
