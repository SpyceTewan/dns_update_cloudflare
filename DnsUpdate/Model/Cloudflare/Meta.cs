using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Cloudflare;

public record Meta
{
    [JsonPropertyName("auto_added")]
    public bool AutoAdded { get; set; }
    [JsonPropertyName("source")]
    public string Source { get; set; }
}