using CommandLine;
using IPTVRelay.Database;

namespace IPTVRelay.Blazor.Configuration
{
    public class IPTVConfig
    {
        [Option('d', "data", Required = false, HelpText = "Data folder")]
        public string DataFolder { get; set; } = "./data";
        [Option('c', "config", Required = false, HelpText = "Config folder")]
        public string ConfigFolder { get; set; } = ".";
        [Option('p', "port", Required = false, HelpText = "Port")]
        public int Port { get; set; } = 8080;
        [Option('h', "host", Required = false, HelpText = "Host")]
        public string Host { get; set; } = "localhost";
        public IPTVRelayDbConfig DbConfig { get; set; } = new();
    }
}
