using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IPTVRelay.Database.Sqlite
{
    internal class IPTVRelaySqliteContextDesignFactory : IDesignTimeDbContextFactory<IPTVRelaySqliteContext>
    {
        public IPTVRelaySqliteContext CreateDbContext(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("IPTV_");
            var config = (IConfiguration)configBuilder
                .Build();

            IPTVRelayDbConfig iptvconfig = new();
            config.Bind(iptvconfig);

            var optionsBuilder = new DbContextOptionsBuilder<IPTVRelaySqliteContext>();
            optionsBuilder.UseSqlite(
                new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    DataSource = iptvconfig.SqliteConfig.Path
                }.ConnectionString,

            x => x.MigrationsAssembly(Constants.SQLITE_MIGRATIONS));

            return new IPTVRelaySqliteContext(optionsBuilder.Options);
        }

    }
}
