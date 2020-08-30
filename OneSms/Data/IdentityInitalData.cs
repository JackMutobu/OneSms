using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OneSms.Constants;
using OneSms.Domain;
using OneSms.Options;
using System.Threading.Tasks;

namespace OneSms.Data
{
    public class IdentityInitalData
    {
        public static async Task SeedData(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager, configuration);
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            var adminSettings = new AdminSettings();
            configuration.Bind(nameof(adminSettings), adminSettings);
            var user = new AppUser
            {
                Email = adminSettings.Email,
                PhoneNumber = adminSettings.Phone,
                Fristname = "OneSms",
                Lastname = "Admin",
                UserName = adminSettings.Username,
                Role = Roles.SuperAdmin
            };
           

            await CreateUserAsync(userManager, user, adminSettings.Password);
        }

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            await CreateRoleAsync(roleManager, Roles.Guest);
            await CreateRoleAsync(roleManager, Roles.Client);
            await CreateRoleAsync(roleManager, Roles.Admin);
            await CreateRoleAsync(roleManager, Roles.SuperAdmin);
            await CreateRoleAsync(roleManager, Roles.ApiUser);
            await CreateRoleAsync(roleManager, Roles.MobileServer);
        }
        public static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
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
        public static async Task CreateUserAsync(UserManager<AppUser> userManager, AppUser user, string password)
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
