using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Configuration;
using Npgsql;
using CommandLine;

namespace IPTVRelay.Database.Psql
{
    internal class IPTVRelayNpgsqlContextDesignFactory : IDesignTimeDbContextFactory<IPTVRelayNpgsqlContext>
    {
        public IPTVRelayNpgsqlContext CreateDbContext(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("IPTV_");

            Console.WriteLine($"Args: {string.Join(", ", args)}");
            var config = new IPTVRelayPsqlConfig();
            Parser.Default.ParseArguments(() => config, args);
            Console.WriteLine($"Config: {config}");

            configBuilder.Build().Bind(config, o =>
            {
                o.ErrorOnUnknownConfiguration = false;
                o.BindNonPublicProperties = true;
            });
            var optionsBuilder = new DbContextOptionsBuilder<IPTVRelayNpgsqlContext>();
                optionsBuilder.UseNpgsql(
                   new NpgsqlConnectionStringBuilder
                   {
                       Host = config.Host,
                       Port = config.Port,
                       Username = config.Username,
                       Password = config.Password,
                       Database = config.Database
                   }.ConnectionString,
                   x => x.MigrationsAssembly(Constants.PSQL_MIGRATIONS))
               ;

            
            return new IPTVRelayNpgsqlContext(optionsBuilder.Options);
        }

    }
}
