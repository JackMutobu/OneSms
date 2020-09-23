using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneSms.Domain;

namespace OneSms.Data
{
    public class DataContext: IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Application> Apps { get; set; } = null!;

        public DbSet<SimCard> Sims { get; set; } = null!;

        public DbSet<MobileServer> MobileServers { get; set; } = null!;

        public DbSet<NetworkOperator> Networks { get; set; } = null!;

        public DbSet<ApplicationSim> AppSims { get; set; } = null!;

        public DbSet<SmsMessage> SmsMessages { get; set; } = null!;

        public DbSet<WhatsappMessage> WhatsappMessages { get; set; } = null!;

        public DbSet<Contact> Contacts { get; set; } = null!;

        public DbSet<AppContact> AppContacts { get; set; } = null!;

        public DbSet<NetworkMessageExtractor> NetworkMessageExtractors { get; set; } = null!;

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

            builder.Entity<AppContact>()
        .HasKey(aps => new { aps.AppId, aps.ContactId });
            builder.Entity<AppContact>()
                .HasOne(bc => bc.App)
                .WithMany(b => b.Contacts)
                .HasForeignKey(bc => bc.AppId);

            builder.Entity<AppContact>()
                .HasOne(bc => bc.Contact)
                .WithMany(c => c.Apps)
                .HasForeignKey(bc => bc.ContactId);
        }
    }
}
