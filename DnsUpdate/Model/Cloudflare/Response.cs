using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Cloudflare;

internal abstract record Response
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = false;
    
    [JsonPropertyName("errors")]
    public string[] Errors { get; set; } = null!;

    [JsonPropertyName("messages")]
    public string[] Messages { get; set; } = null!;
    
}