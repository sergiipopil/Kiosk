using System.Data.Common;
using System.Linq;
using KioskBrains.Server.Domain.Entities.EK;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Entities.DbStorage
{
    public class KioskBrainsContext : DbContext
    {
        public KioskBrainsContext(DbContextOptions<KioskBrainsContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<CentralBankExchangeRate> CentralBankExchangeRates { get; set; }

        public DbSet<CentralBankExchangeRateUpdate> CentralBankExchangeRateUpdates { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<DbPersistentCacheItem> DbPersistentCacheItems { get; set; }

        public DbSet<IntegrationLogRecord> IntegrationLogRecords { get; set; }

        public DbSet<Kiosk> Kiosks { get; set; }

        public DbSet<KioskState> KioskStates { get; set; }

        public DbSet<KioskStateComponentInfo> KioskStateComponentInfos { get; set; }

        public DbSet<KioskVersionUpdate> KioskVersionUpdates { get; set; }

        public DbSet<LogRecord> LogRecords { get; set; }

        public DbSet<NotificationCallbackLogRecord> NotificationCallbackLogRecords { get; set; }

        public DbSet<NotificationReceiver> NotificationReceivers { get; set; }

        public DbSet<PortalUser> PortalUsers { get; set; }

        public DbSet<State> States { get; set; }

        #region EK

        public DbSet<EkImageCacheItem> EkImageCacheItems { get; set; }

        public DbSet<EkTransaction> EkTransactions { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CentralBankExchangeRateUpdate>()
                .Property(x => x.Rate)
                .HasColumnType("decimal(19,6)");

            modelBuilder.Entity<DbPersistentCacheItem>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<IntegrationLogRecord>()
                .HasIndex(x => x.ExternalSystem);

            modelBuilder.Entity<IntegrationLogRecord>()
                .HasIndex(x => x.RequestedOnUtc);

            modelBuilder.Entity<Kiosk>()
                .HasIndex(x => x.SerialKey)
                .IsUnique();

            modelBuilder.Entity<KioskVersionUpdate>()
                .HasIndex(x => new
                {
                    x.VersionName,
                })
                .IsUnique();

            modelBuilder.Entity<LogRecord>()
                .HasIndex(x => x.Type);

            modelBuilder.Entity<LogRecord>()
                .HasIndex(x => x.Context);

            modelBuilder.Entity<LogRecord>()
                .HasIndex(x => x.LocalTime);

            modelBuilder.Entity<PortalUser>()
                .HasIndex(x => x.Username)
                .IsUnique();

            #region EK

            modelBuilder.Entity<EkTransaction>()
                .HasIndex(x => x.LocalStartedOn);

            modelBuilder.Entity<EkTransaction>()
                .Property(x => x.TotalPrice)
                .HasColumnType("decimal(19,2)");

            modelBuilder.Entity<EkTransaction>()
                .HasIndex(x => new
                {
                    x.CompletionStatus,
                    x.IsSentToEkSystem,
                    x.NextSendingToEkTimeUtc,
                });

            modelBuilder.Entity<EkImageCacheItem>()
                .HasIndex(x => new
                {
                    x.ImageKey,
                })
                .IsUnique();

            #endregion

            DisableCascadeDelete(modelBuilder);
        }

        // Dapper Helpers
        public DbConnection Connection => Database.GetDbConnection();

        private void DisableCascadeDelete(ModelBuilder modelBuilder)
        {
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}