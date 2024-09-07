using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace IPTVRelay.Database.Psql
{
    public class IPTVRelayNpgsqlContextFactory : IDbContextFactory<IPTVRelayNpgsqlContext>
    {
        private readonly DbContextOptions<IPTVRelayNpgsqlContext> _options;

        public IPTVRelayNpgsqlContextFactory(DbContextOptions<IPTVRelayNpgsqlContext> options)
        {
            _options = options;
        }
        public IPTVRelayNpgsqlContextFactory(IPTVRelayDbConfig iptvconfig)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IPTVRelayNpgsqlContext>();
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

        public IPTVRelayNpgsqlContext CreateDbContext()
        {
            return new IPTVRelayNpgsqlContext(new DbContextOptionsBuilder<IPTVRelayNpgsqlContext>(_options).Options);
        }

    }
}
