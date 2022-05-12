using System;
using System.IO;
using System.Text.Json;
using DnsUpdate.Model.Config;

namespace DnsUpdate;

public static class Program
{

    /// <param name="config">Path to the config file to load</param>
    /// <param name="force">Force update the DNS records even if the cached IP didn't change</param>
    /// <param name="dump">List the records and all metadata, like the ID</param>
    public static void Main(string config = "./config.json", bool force = false, bool dump = false)
    {
        Config? c = LoadConfig(config);
        if (c is null)
        {
            Console.Error.WriteLine("Cannot load config found at '{0}'", Path.GetFullPath(config));
            return;
        }

        if (dump)
        {
            new CloudflareDnsDumper(c).Execute().Wait();
            return;
        }

        new CloudflareDnsUpdater(c, force).Execute().Wait();
    }

    private static Config? LoadConfig(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Config>(File.ReadAllText(path));
    }
    
}