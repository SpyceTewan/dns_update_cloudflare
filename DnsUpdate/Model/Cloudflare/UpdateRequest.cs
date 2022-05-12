using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Cloudflare;

public record UpdateRequest {
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }
    [JsonPropertyName("ttl")]
    public int TimeToLive { get; set; }
    
}