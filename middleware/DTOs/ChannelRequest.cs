using System.Text.Json.Serialization;

namespace middleware.DTOs;

public class ChannelRequest
{
    [JsonPropertyName("resource-name")]
    public string ResourceName { get; set; } = "";

    [JsonPropertyName("resource-desc")]
    public string ResourceDesc { get; set; } = "";

    [JsonPropertyName("res-type")]
    public string? ResType {get; set;}
}

