using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DnsUpdate;

public class IpQuery
{

    private const string CacheIpName = "lastip";

    private readonly HttpClient _client;
    private readonly CacheManager _cache;
    public string? Ip { get; set; }

    internal IpQuery(HttpClient client, CacheManager cacheManager)
    {
        _client = client;
        _cache = cacheManager;
    }

    public async Task QueryIp()
    {
        HttpRequestMessage myIpMessage = new HttpRequestMessage
        {
            RequestUri = new Uri("https://diagnostic.opendns.com/myip")
        };

        var response = await _client.SendAsync(myIpMessage);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Console.Error.WriteLine("Failed to get public IP. Code {0}", response.StatusCode);
            return;
        }

        string ip = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Public IP: {0}", ip);
        
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