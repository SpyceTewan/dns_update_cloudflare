using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DnsUpdate.Model.Cloudflare;
using DnsUpdate.Model.Config;

namespace DnsUpdate;

internal class CloudflareDnsDumper
{

    private readonly Config _config;

    public CloudflareDnsDumper(Config config)
    {
        _config = config;
    }
    
    public async Task Execute()
    {
        HttpClient httpClient = new HttpClient();
        Console.WriteLine("Dumping zone record metadata");
        foreach (var zone in _config.Zones)
        {
            Console.WriteLine(zone.Name);
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
                Console.Error.WriteLine("Failed to retrieve zone information for {0} | {1}: {2}", zone.Name, response.StatusCode, response.ReasonPhrase);
                return;
            }
            var dnsList = await response.Content.ReadFromJsonAsync<DnsListResponse>();
            if (dnsList is null)
            {
                Console.Error.WriteLine("Failed to parse zone information for {0} | {1}: {2}", zone, response.StatusCode, response.ReasonPhrase);
                return;
            }
            foreach (var record in dnsList.Result)
            {
                Console.WriteLine($"  Id:        {record.Id}");
                Console.WriteLine($"  Type:      {record.Type}");
                Console.WriteLine($"  Name:      {record.Name}");
                Console.WriteLine($"  Content:   {record.Content}");
                Console.WriteLine($"  Created:   {record.CreatedOn}");
                Console.WriteLine($"  Modified:  {record.ModifiedOn}");
                Console.WriteLine($"  Ttl:       {record.TimeToLive}");
                Console.WriteLine($"  Proxied:   {record.Proxied}");
                Console.WriteLine("----------------------------------------");
            }
            Console.WriteLine("");
        }
    }
}