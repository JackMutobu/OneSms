using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneSms.Domain;
using OneSms.Models;
using OneSms.Web.Shared.Models;
using SimCard = OneSms.Web.Shared.Models.SimCard;

namespace OneSms.Data
{
    public class OneSmsDbContext : IdentityDbContext<OneSmsUser>
    {
        public OneSmsDbContext(DbContextOptions<OneSmsDbContext> options) : base(options)
        {
        }
        public DbSet<Web.Shared.Models.NetworkOperator> Networks { get; set; }
        public DbSet<OneSmsApp> Apps { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ServerMobile> MobileServers { get; set; }
        public DbSet<SimCard> Sims { get; set; }
        public DbSet<UssdAction> UssdActions { get; set; }
        public DbSet<UssdActionStep> UssdActionSteps { get; set; }
        public DbSet<UssdTransaction> UssdTransactions { get; set; }
        public DbSet<SmsTransaction> SmsTransactions { get; set; }
        public DbSet<AppSim> AppSims { get; set; }
        public DbSet<SmsDataExtractor> SmsDataExtractors { get; set; }
        public DbSet<WhatsappTransaction> WhatsappTransactions { get; set; }

        public DbSet<TimClient> TimClients { get; set; }
        public DbSet<TimTransaction> TimTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AppSim>()
        .HasKey(aps => new { aps.AppId, aps.SimId });
            builder.Entity<AppSim>()
                .HasOne(bc => bc.App)
                .WithMany(b => b.Sims)
                .HasForeignKey(bc => bc.AppId);
            builder.Entity<AppSim>()
                .HasOne(bc => bc.Sim)
                .WithMany(c => c.Apps)
                .HasForeignKey(bc => bc.SimId);
        }
    }
}
