using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Models;
using OneSms.Options;
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
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IMessageExtractionService, MessageExtractionService>();
            services.AddScoped<INetworkMessageExtractionService, NetworkMessageExtractionService>();
            services.AddScoped<ISimCardManagementService, SimCardManagementService>();
            services.AddScoped<INetworkService, NetworkService>();
            services.AddScoped<SmsDataExtractorService>();
            services.AddSingleton<HubEventService>();
            services.AddScoped<TimService>();
            services.AddScoped<SimService>();

            var urlOptions = new UrlOptions();
            configuration.GetSection(nameof(UrlOptions)).Bind(urlOptions);
            services.AddSingleton<IUriService>(provider =>
            {
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
                return new UriService(absoluteUri,urlOptions.InternetUrl);
            });
        }
    }
}
