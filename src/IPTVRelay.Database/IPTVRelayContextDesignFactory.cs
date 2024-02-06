using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace IPTVRelay.Database
{
    internal class IPTVRelayContextDesignFactory : IDesignTimeDbContextFactory<IPTVRelayContext>
    {
        public IPTVRelayContext CreateDbContext(string[] args)
        {
            var connectionString = new SqliteConnectionStringBuilder()
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                DataSource = "iptvrelay.db"

            }.ToString();
            var optionsBuilder = new DbContextOptionsBuilder<IPTVRelayContext>();
            optionsBuilder.UseSqlite(connectionString);
            return new IPTVRelayContext(optionsBuilder.Options);
        }

    }
}
