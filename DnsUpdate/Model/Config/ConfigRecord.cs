using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Config;

internal record ConfigRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("ttl")]
    public int TimeToLive { get; set; } = 0;
    
    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; } = false;
}