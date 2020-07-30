using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OneSms.Online.Models;
using OneSms.Web.Shared.Constants;
using System.Threading.Tasks;

namespace OneSms.Online.Data
{
    public class IdentityDbInitializer
    {
        public static async Task SeedData(UserManager<OneSmsUser> userManager, RoleManager<IdentityRole> roleManager,IConfiguration configuration)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager,configuration);
        }

        public static async Task SeedUsers(UserManager<OneSmsUser> userManager,IConfiguration configuration)
        {
            var user = new OneSmsUser
            {
                Email = configuration!.GetSection("AdminSettings")["Email"],
                PhoneNumber = configuration.GetSection("AdminSettings")["Phone"],
                Fristname = "OneSms",
                Lastname = "Admin",
                UserName = configuration!.GetSection("AdminSettings")["Email"],
                Role = UserRoles.SuperAdmin
            };

            await CreateUserAsync(userManager, user, configuration.GetSection("AdminSettings")["Password"]);
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            await CreateRoleAsync(roleManager, UserRoles.Guest);
            await CreateRoleAsync(roleManager, UserRoles.Client);
            await CreateRoleAsync(roleManager, UserRoles.Admin);
            await CreateRoleAsync(roleManager, UserRoles.SuperAdmin);
        }
        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                IdentityRole role = new IdentityRole
                {
                    Name = roleName
                };
                await roleManager.CreateAsync(role);
            }
        }
        private static async Task CreateUserAsync(UserManager<OneSmsUser> userManager, OneSmsUser user, string password)
        {
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                IdentityResult result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, user.Role);
                }
            }
        }
    }
}
