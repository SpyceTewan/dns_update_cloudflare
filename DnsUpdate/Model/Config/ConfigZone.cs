using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Config;

internal record ConfigZone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;

    [JsonPropertyName("records")]
    public List<ConfigRecord> Records { get; set; } = null!;
}