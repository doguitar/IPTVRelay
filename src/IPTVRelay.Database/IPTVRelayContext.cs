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
                .Select(e => new { e.State, Entity = e.Entity as ModelBase })
                .Where(e => e.Entity != null && e.State != EntityState.Unchanged).ToList();

            var changed = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return changed;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entries = ChangeTracker
                .Entries()
                .Select(e => new { e.State, Entity = e.Entity as ModelBase })
                .Where(e => e.Entity != null && e.State != EntityState.Unchanged).ToList();

            var changed = base.SaveChanges(acceptAllChangesOnSuccess);

            return changed;
        }


		public DbSet<Models.M3U> M3U { get; set; }
		public DbSet<Models.M3UItem> M3UItem { get; set; }
		public DbSet<Models.M3UItemData> M3UItemData { get; set; }
		public DbSet<Models.XMLTV> XMLTV { get; set; }
		public DbSet<Models.XMLTVItem> XMLTVItem { get; set; }
		public DbSet<Models.XMLTVItemData> XMLTVItemsData { get; set; }
        public DbSet<Models.Settings> Settings { get; set; }
        public DbSet<Models.Setting> Setting { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Models.M3U>()
               .HasIndex(m => m.Id)
               .IsUnique();

            modelBuilder.Entity<Models.M3UItem>()
               .HasIndex(m => m.Id)
               .IsUnique();
            modelBuilder.Entity<Models.M3UItem>()
               .HasIndex(m => m.M3UId);

            modelBuilder.Entity<Models.M3UItemData>()
               .HasIndex(m => m.Id)
               .IsUnique();
            modelBuilder.Entity<Models.M3UItemData>()
               .HasIndex(m => m.M3UItemId);


            modelBuilder.Entity<Models.XMLTV>()
               .HasIndex(m => m.Id)
               .IsUnique();

            modelBuilder.Entity<Models.XMLTVItem>()
               .HasIndex(m => m.Id)
               .IsUnique();
            modelBuilder.Entity<Models.XMLTVItem>()
               .HasIndex(m => m.XMLTVId);

            modelBuilder.Entity<Models.XMLTVItemData>()
               .HasIndex(m => m.Id)
               .IsUnique();
            modelBuilder.Entity<Models.XMLTVItemData>()
               .HasIndex(m => m.XMLTVItemId);

            

            modelBuilder.Entity<Models.Settings>()
               .HasIndex(m => m.Id)
               .IsUnique();

            modelBuilder.Entity<Models.Setting>()
               .HasIndex(m => m.Id)
               .IsUnique();
            modelBuilder.Entity<Models.Setting>()
               .HasIndex(m => m.SettingsId);

            base.OnModelCreating(modelBuilder);
        }
    }

}