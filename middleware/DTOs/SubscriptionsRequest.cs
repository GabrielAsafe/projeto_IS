using System.Text.Json.Serialization;

namespace middleware.DTOs;

public class SubscriptionsRequest
{
    [JsonPropertyName("res-type")]
    public string? ResType { get; set; }

    [JsonPropertyName("resource-name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonPropertyName("evt")]
    public int? Evt { get; set; }

    [JsonPropertyName("dt-expiration")]
    public string? DtExpiration { get; set; }
}