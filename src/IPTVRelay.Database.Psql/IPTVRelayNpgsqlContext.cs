using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPTVRelay.Database.Psql
{
    public class IPTVRelayNpgsqlContext : IPTVRelayContext<IPTVRelayNpgsqlContext, DbContextOptions<IPTVRelayNpgsqlContext>>
    {
        public IPTVRelayNpgsqlContext(DbContextOptions<IPTVRelayNpgsqlContext> options)
            : base(options)
        {
        }

        protected override void SetAutoNumberForPrimaryKey(PropertyBuilder primaryKey)
        {
            primaryKey
                .UseIdentityAlwaysColumn();
        }
    }

}