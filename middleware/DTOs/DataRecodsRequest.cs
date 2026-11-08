using System.Text.Json.Serialization;

namespace middleware.DTOs;

public class DataRecordRequest
{
    [JsonPropertyName("res-type")]
    public string? ResType { get; set; }

    [JsonPropertyName("resource-name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("content-type")]
    public string ContentType { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("dt-expiration")]
    public string? DtExpiration { get; set; }
}