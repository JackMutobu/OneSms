using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneSms.Domain;
using OneSms.Online.Areas.Identity;

namespace OneSms.Installers
{
    public class FrameworkInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
            services.AddControllers();
            services.AddSignalR();

            services.AddAntDesign();

            services.AddHttpContextAccessor();
        }
    }
}
