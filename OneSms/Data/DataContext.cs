using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneSms.Domain;

namespace OneSms.Data
{
    public class DataContext: IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
             
        public DbSet<Application> Apps { get; set; }

        public DbSet<SimCard> Sims { get; set; }

        public DbSet<MobileServer> MobileServers { get; set; }

        public DbSet<NetworkOperator> Networks { get; set; }

        public DbSet<ApplicationSim> AppSims { get; set; }

        public DbSet<SmsMessage> SmsMessages { get; set; }

        public DbSet<WhatsappMessage> WhatsappMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationSim>()
        .HasKey(aps => new { aps.AppId, aps.SimId });
            builder.Entity<ApplicationSim>()
                .HasOne(bc => bc.App)
                .WithMany(b => b.Sims)
                .HasForeignKey(bc => bc.AppId);

            builder.Entity<ApplicationSim>()
                .HasOne(bc => bc.Sim)
                .WithMany(c => c.Apps)
                .HasForeignKey(bc => bc.SimId);
        }
    }
}
