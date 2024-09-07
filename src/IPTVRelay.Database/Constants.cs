using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Database
{
    public static class Constants
    {
        public const string IPTV_PREFIX = "IPTV_";
        public const string PSQL_PREFIX = "PSQL__";

        public const string PSQL_HOST = $"{PSQL_PREFIX}HOST";
        public const string PSQL_PORT = $"{PSQL_PREFIX}PORT";
        public const string PSQL_DB = $"{PSQL_PREFIX}DB";
        public const string PSQL_USER = $"{PSQL_PREFIX}USER";
        public const string PSQL_PASS = $"{PSQL_PREFIX}PASS";

        public const string SQLITE_PREFIX = $"SQLITE__";
        public const string SQLITE_PATH = $"{SQLITE_PREFIX}PATH";

        public const string PSQL_MIGRATIONS = $"{nameof(IPTVRelay)}.{nameof(Database)}.{nameof(DbProvider.Psql)}";
        public const string SQLITE_MIGRATIONS = $"{nameof(IPTVRelay)}.{nameof(Database)}.{nameof(DbProvider.Sqlite)}";
    }

    public record IPTVRelayDbConfig
    {
        public DbProvider Provider { get; set; } = DbProvider.Sqlite;

        [ConfigurationKeyName("Psql")]
        public IPTVRelayPsqlConfig PsqlConfig { get; } = new();

        [ConfigurationKeyName("Sqlite")]
        public IPTVRelaySqliteConfig SqliteConfig { get; } = new();
    }
    public enum DbProvider
    {
        Psql,
        Sqlite
    }

    public class IPTVRelayPsqlConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "iptvrelay";
        public string Username { get; set; } = "iptvrelay";
        public string Password { get; set; } = "iptvrelay";
    }
    public class IPTVRelaySqliteConfig
    {
        public string Path { get; set; } = "iptvrelay.db";
    }

}
