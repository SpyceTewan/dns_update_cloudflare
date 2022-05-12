using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DnsUpdate.Model.Cloudflare;
using DnsUpdate.Model.Config;
using Serilog;

namespace DnsUpdate;

internal class CloudflareDnsDumper
{
    private readonly ILogger _logger;
    private readonly Config _config;

    public CloudflareDnsDumper(ILogger logger, Config config)
    {
        _logger = logger;
        _config = config;
    }
    
    public async Task Execute()
    {
        HttpClient httpClient = new HttpClient();
        _logger.Information("Dumping zone record metadata");
        foreach (var zone in _config.Zones)
        {
            _logger.Information("Dump for {Name}", zone.Name);
            HttpRequestMessage message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{zone.Id}/dns_records"),
                Headers =
                {
                    { "Authorization", $"Bearer {zone.Token}" }
                }
            };
            
            var response = await httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Failed to retrieve zone information for {Name} | {StatusCode}: {Reason}", zone.Name, response.StatusCode, response.ReasonPhrase);
                return;
            }
            var dnsList = await response.Content.ReadFromJsonAsync<DnsListResponse>();
            if (dnsList is null)
            {
                _logger.Error("Failed to parse zone information for {Name} | {StatusCode}: {Reason}", zone.Name, response.StatusCode, response.ReasonPhrase);
                return;
            }
            foreach (var record in dnsList.Result)
            {
                _logger.Information("  Id:        {Id}", record.Id);
                _logger.Information("  Type:      {Type}", record.Type);
                _logger.Information("  Name:      {Name}", record.Name);
                _logger.Information("  Content:   {Content}", record.Content);
                _logger.Information("  Created:   {CreatedOn}", record.CreatedOn);
                _logger.Information("  Modified:  {ModifiedOn}", record.ModifiedOn);
                _logger.Information("  Ttl:       {Ttl}", record.TimeToLive);
                _logger.Information("  Proxied:   {Proxied}", record.Proxied);
                _logger.Information("----------------------------------------");
            }
        }
    }
}