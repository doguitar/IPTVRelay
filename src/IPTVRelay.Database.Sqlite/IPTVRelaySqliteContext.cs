using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace IPTVRelay.Database.Sqlite
{
    public class IPTVRelaySqliteContext : IPTVRelayContext<IPTVRelaySqliteContext, DbContextOptions<IPTVRelaySqliteContext>>
    {
        public IPTVRelaySqliteContext(DbContextOptions<IPTVRelaySqliteContext> options)
            : base(options)
        {
        }

        protected override void SetAutoNumberForPrimaryKey(PropertyBuilder primaryKey)
        {
            primaryKey
                .IsRequired()
                .ValueGeneratedOnAdd();
        }
    }
}