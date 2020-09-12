using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;
using OneSms.Domain;
using OneSms.Hubs;
using OneSms.Online.Areas.Identity;
using System;

namespace OneSms.Installers
{
    public class FrameworkInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
            services.AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
            services.AddSignalR().AddHubOptions<OneSmsHub>(options =>
             {
                 options.EnableDetailedErrors = true;
             });
            services.AddAntDesign();

            services.AddHttpContextAccessor();
        }
    }
}
