using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneSms.Data;
using OneSms.Domain;
using System.Threading.Tasks;

namespace OneSms
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var serviceScope = host.Services.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

            await dbContext.Database.MigrateAsync();

            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

            await IdentityInitalData.SeedData(userManager, roleManager, config);

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
