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
    public class IPTVRelayContextFactory : IDbContextFactory<IPTVRelayContext>
    {
        private readonly DbContextOptions<IPTVRelayContext> _options;

        public IPTVRelayContextFactory(DbContextOptions<IPTVRelayContext> options)
        {
            _options = options;
        }

        public IPTVRelayContext CreateDbContext()
        {
            return new IPTVRelayContext(new DbContextOptionsBuilder<IPTVRelayContext>(_options).Options);
        }

    }
}
