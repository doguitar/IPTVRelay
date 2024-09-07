using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace IPTVRelay.Database
{
    public class IPTVRelayContextFactory : IDbContextFactory<IPTVRelayContext>
    {
        private readonly DbContextOptions<IPTVRelayContext> _options;

        public IPTVRelayContextFactory(DbContextOptions<IPTVRelayContext> options)
        {
            _options = options;
        }
        public IPTVRelayContextFactory(IPTVRelayDbConfig iptvconfig)
        {
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
            _options = optionsBuilder.Options;
        }

        public IPTVRelayContext CreateDbContext()
        {
            return new IPTVRelayContext(new DbContextOptionsBuilder<IPTVRelayContext>(_options).Options);
        }

    }
}
