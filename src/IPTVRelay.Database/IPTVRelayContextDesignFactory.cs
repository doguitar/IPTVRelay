using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IPTVRelay.Database
{
    internal class IPTVRelayContextDesignFactory : IDesignTimeDbContextFactory<IPTVRelayContext>
    {
        public IPTVRelayContext CreateDbContext(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("IPTV_");
            var config = (IConfiguration)configBuilder
                .Build();

            IPTVRelayDbConfig iptvconfig = new();
            config.Bind(iptvconfig);

            var optionsBuilder = new DbContextOptionsBuilder<IPTVRelayContext>();
            if (iptvconfig.Provider == DbProvider.Psql)
            {
                optionsBuilder.UseNpgsql(
                   new NpgsqlConnectionStringBuilder
                   {
                       Host = iptvconfig.PsqlConfig.Host,
                       Port = iptvconfig.PsqlConfig.Port,
                       Username = iptvconfig.PsqlConfig.Username,
                       Password = iptvconfig.PsqlConfig.Password,
                       Database = iptvconfig.PsqlConfig.Database
                   }.ConnectionString,
                   x => x.MigrationsAssembly(Constants.PSQL_MIGRATIONS))
               ;

            }
            else if (iptvconfig.Provider == DbProvider.Sqlite)
            {
                optionsBuilder.UseSqlite(
                    new SqliteConnectionStringBuilder
                    {
                        Mode = SqliteOpenMode.ReadWriteCreate,
                        DataSource = iptvconfig.SqliteConfig.Path
                    }.ConnectionString,

                x => x.MigrationsAssembly(Constants.SQLITE_MIGRATIONS));
            }
            return new IPTVRelayContext(optionsBuilder.Options);
        }

    }
}
