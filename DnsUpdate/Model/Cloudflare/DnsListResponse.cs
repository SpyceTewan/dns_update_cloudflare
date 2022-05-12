using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DnsUpdate.Model.Cloudflare;

internal record DnsListResponse : Response
{
    [JsonPropertyName("result")]
    public List<DnsListResult> Result { get; set; }
}