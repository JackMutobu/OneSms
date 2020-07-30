using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OneSms.Web.Server.Models;
using OneSms.Web.Shared.Models;

namespace OneSms.Web.Server.Data
{
    public class OneSmsDbContext: ApiAuthorizationDbContext<OneSmsUser>
    {
        public OneSmsDbContext(DbContextOptions options,IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
        public DbSet<NetworkOperator> Networks { get; set; }
        public DbSet<OneSmsApp> Apps { get; set; }
        public DbSet<ServerMobile> MobileServers { get; set; }
        public DbSet<SimCard> Sims { get; set; }
        public DbSet<UssdAction> UssdActions { get; set; }
        public DbSet<UssdActionStep> UssdActionSteps { get; set; }
        public DbSet<UssdTransaction> UssdTransactions { get; set; }
        public DbSet<SmsTransaction> SmsTransactions { get; set; }
    }
}
