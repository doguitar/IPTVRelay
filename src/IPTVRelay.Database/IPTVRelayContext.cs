using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IPTVRelay.Database
{
    public class IPTVRelayContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public IPTVRelayContext(DbContextOptions<IPTVRelayContext> options)
            : base(options)
        {
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity != null && e.State == EntityState.Modified)
                .Select(e => e.Entity as ModelBase)
                .ToList();

            var now = DateTime.UtcNow;
            await Task.WhenAll(entries.Select(e => Task.Run(() => e!.Modified = now)));

            var changed = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return changed;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity != null && e.State == EntityState.Modified)
                .Select(e => e.Entity as ModelBase)
                .ToList();

            var now = DateTime.UtcNow;
            Task.WaitAll(entries.Select(e => Task.Run(() => e!.Modified = now)).ToArray());

            var changed = base.SaveChanges(acceptAllChangesOnSuccess);

            return changed;
        }


        public DbSet<Models.M3U> M3U { get; set; }
        public DbSet<Models.M3UFilter> M3UFilter { get; set; }
        public DbSet<Models.XMLTV> XMLTV { get; set; }
        public DbSet<Models.XMLTVItem> XMLTVItem { get; set; }
        public DbSet<Models.Mapping> Mapping { get; set; }
        public DbSet<Models.MappingFilter> MappingFilter { get; set; }
        public DbSet<Models.Setting> Setting { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Models.M3U>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
            });
            modelBuilder.Entity<Models.M3UFilter>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
                e.HasIndex(m => m.M3UId);
            });

            modelBuilder.Entity<Models.XMLTV>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
            });
            modelBuilder.Entity<Models.XMLTVItem>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
                e.HasIndex(m => m.XMLTVId);
            });

            modelBuilder.Entity<Models.Mapping>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
            });
            modelBuilder.Entity<Models.MappingFilter>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
                e.HasIndex(m => m.MappingId);
            });

            modelBuilder.Entity<Models.Setting>(e =>
            {
                e.HasIndex(m => m.Id).IsUnique();
            });


            base.OnModelCreating(modelBuilder);
        }
    }

}