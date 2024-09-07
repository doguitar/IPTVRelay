using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace IPTVRelay.Database.Sqlite
{
    public class IPTVRelaySqliteContextFactory : IDbContextFactory<IPTVRelaySqliteContext>
    {
        private readonly DbContextOptions<IPTVRelaySqliteContext> _options;

        public IPTVRelaySqliteContextFactory(DbContextOptions<IPTVRelaySqliteContext> options)
        {
            _options = options;
        }
        public IPTVRelaySqliteContextFactory(IPTVRelayDbConfig iptvconfig)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IPTVRelaySqliteContext>();
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

        public IPTVRelaySqliteContext CreateDbContext()
        {
            return new IPTVRelaySqliteContext(new DbContextOptionsBuilder<IPTVRelaySqliteContext>(_options).Options);
        }

    }
}
