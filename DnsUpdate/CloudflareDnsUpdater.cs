using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DnsUpdate.Model.Cloudflare;
using DnsUpdate.Model.Config;
using Serilog;

namespace DnsUpdate;

internal class CloudflareDnsUpdater
{
    private readonly ILogger _logger;
    private readonly Config _config;
    private readonly HttpClient _httpClient;
    private readonly CacheManager _cache;
    private readonly bool _forceUpdate;
    
    public CloudflareDnsUpdater(ILogger logger, Config config, bool force)
    {
        _logger = logger;
        _config = config;
        _forceUpdate = force;
        
        _httpClient = new HttpClient();
        _cache = new CacheManager();
        _cache.EnsureCacheCreated();
    }
    
    public async Task Execute()
    {
        var ipQuery = new IpQuery(_logger, _httpClient, _cache);
        await ipQuery.QueryIp();
        if (!_forceUpdate && !ipQuery.HasIpChanged())
        {
            _logger.Information("IP has not changed. Nothing to do. Clear cache file {FilePath} to force an update", _cache.GetCacheFilePath());
            return;
        }
        var ip = ipQuery.Ip;
        if (ip is null)
        {
            _logger.Error("Failed to retrieve public IP address");
            return;
        }
        ipQuery.StoreIpInCache();

        // Put all tasks into a list and wait for all to be completed
        var recordCount = _config.Zones.SelectMany(z => z.Records).Count();

        List<Task> tasks = new List<Task>();
        _logger.Information("Starting to update {Count} records simultaneously", recordCount);
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
        _logger.Information("Done in {Time} seconds", timeDifference.TotalSeconds);
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
            _logger.Information("Updated {Type} {Name} to point to {Ip}", record.Type, record.Name, ip);
        }
        else
        {
            _logger.Error("Failed to update {Type} {Name} to point to {Ip} | {StatusCode}: {Reason}", record.Type, record.Name, ip, updateResponse.StatusCode, updateResponse.ReasonPhrase);
        }
    }
}