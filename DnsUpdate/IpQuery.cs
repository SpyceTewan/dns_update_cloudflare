using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;

namespace DnsUpdate;

public class IpQuery
{

    private const string CacheIpName = "lastip";

    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly CacheManager _cache;
    public string? Ip { get; set; }

    internal IpQuery(ILogger logger, HttpClient client, CacheManager cacheManager)
    {
        _logger = logger;
        _client = client;
        _cache = cacheManager;
    }

    public async Task QueryIp()
    {
        HttpRequestMessage myIpMessage = new HttpRequestMessage
        {
            RequestUri = new Uri("https://api.ipify.org")
        };

        var response = await _client.SendAsync(myIpMessage);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.Error("Failed to get public IP. Code {StatusCode}", response.StatusCode);
            return;
        }

        string ip = await response.Content.ReadAsStringAsync();
        _logger.Information("Public IP: {Ip}", ip);
        
        Ip = ip;
    }
    
    public bool HasIpChanged()
    {
        if (Ip is null)
        {
            throw new Exception($"{nameof(HasIpChanged)} has been called before ip was queried using {nameof(QueryIp)}");
        }
        var lastIp = _cache.Read(CacheIpName);
        return lastIp != Ip;
    }

    public void StoreIpInCache()
    {
        _cache.Write(CacheIpName, Ip);
    }
}