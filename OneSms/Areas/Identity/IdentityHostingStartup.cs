using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(OneSms.Online.Areas.Identity.IdentityHostingStartup))]
namespace OneSms.Online.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {

            });
        }
    }
}