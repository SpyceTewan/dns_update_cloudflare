using System;
using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Cloudflare;

internal record DnsListResult
{
    [JsonPropertyName("data")]
    public Object Data { get; set; } = null!;
    
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = null!;
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;

    [JsonPropertyName("proxiable")]
    public bool Proxiable { get; set; } = false;
    
    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; } = false;
    
    [JsonPropertyName("ttl")]
    public int TimeToLive { get; set; } = 0;
    
    [JsonPropertyName("locked")]
    public bool Locked { get; set; } = false;
    
    [JsonPropertyName("zone_id")]
    public string ZoneId { get; set; } = null!;
    
    [JsonPropertyName("zone_name")]
    public string ZoneName { get; set; } = null!;
    
    [JsonPropertyName("created_on")]
    public string CreatedOn { get; set; } = null!;
    
    [JsonPropertyName("modified_on")]
    public string ModifiedOn { get; set; } = null!;
}