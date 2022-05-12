using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Config;

internal record Config
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = null!;
    
    [JsonPropertyName("zones")]
    public List<ConfigZone> Zones { get; set; } = null!;
}