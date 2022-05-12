using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DnsUpdate.Model.Cloudflare;
using DnsUpdate.Model.Config;

namespace DnsUpdate;

internal class CloudflareDnsUpdater
{
    private readonly Config _config;
    private readonly HttpClient _httpClient;
    private readonly CacheManager _cache;
    private readonly bool _forceUpdate;
    
    public CloudflareDnsUpdater(Config config, bool force)
    {
        _config = config;
        _forceUpdate = force;
        
        _httpClient = new HttpClient();
        _cache = new CacheManager();
        _cache.EnsureCacheCreated();
    }
    
    public async Task Execute()
    {
        var ipQuery = new IpQuery(_httpClient, _cache);
        await ipQuery.QueryIp();
        if (!_forceUpdate && !ipQuery.HasIpChanged())
        {
            Console.WriteLine("IP has not changed. Nothing to do. Clear cache file {0} to force an update", _cache.GetCacheFilePath());
            return;
        }
        var ip = ipQuery.Ip;
        if (ip is null)
        {
            Console.Error.WriteLine("Failed to retrieve public IP address");
            return;
        }
        ipQuery.StoreIpInCache();

        // Put all tasks into a list and wait for all to be completed
        var recordCount = _config.Zones.SelectMany(z => z.Records).Count();

        List<Task> tasks = new List<Task>();
        Console.WriteLine("Starting to update {0} records simultaneously", recordCount);
        DateTime timeStart = DateTime.Now;
        foreach (var zone in _config.Zones)
        {
            foreach (var record in zone.Records)
            {
                tasks.Add(UpdateRecord(record, zone, ip));
            }
        }

        await Task.WhenAll(tasks);
        TimeSpan timeDifference = DateTime.Now - timeStart;
        Console.WriteLine("Done in {0} seconds", timeDifference.TotalSeconds);
    }

    private async Task UpdateRecord(ConfigRecord record, ConfigZone zone, string ip)
    {
        JsonContent content = JsonContent.Create(new UpdateRequest
        {
            Type = record.Type,
            Name = record.Name,
            Content = ip,
            TimeToLive = record.TimeToLive,
            Proxied = record.Proxied
        });
        HttpRequestMessage message = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{zone.Id}/dns_records/{record.Id}"),
            Headers =
            {
                { "Authorization", $"Bearer {zone.Token}" }
            },
            Content = content
        };
            
        var updateResponse = await _httpClient.SendAsync(message);
        
        if (updateResponse.StatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine("Updated {0} {1} to point to {2}", record.Type, record.Name, ip);
        }
        else
        {
            Console.Error.WriteLine("Failed to update {0} {1} to point to {2} | {3}: {4}", record.Type, record.Name, ip, updateResponse.StatusCode, updateResponse.ReasonPhrase);
        }
    }
}