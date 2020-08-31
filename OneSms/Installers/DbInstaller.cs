using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Online.Models;
using OneSms.Online.Services;
using OneSms.Services;

namespace OneSms.Installers
{
    public class DbInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OneSmsDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


            services.AddDbContext<DataContext>(options =>
              options.UseSqlServer(configuration.GetConnectionString("PrimaryConnection")));

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
              .AddEntityFrameworkStores<DataContext>()
              .AddDefaultTokenProviders();


            services.AddSingleton<IServerConnectionService, ServerConnectionService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IWhatsappService, WhatsappService>();
            services.AddSingleton<HubEventService>();
            services.AddScoped<TimService>();
            services.AddScoped<SmsDataExtractorService>();
            services.AddScoped<SimService>();
        }
    }
}
