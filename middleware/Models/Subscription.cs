
using System.Text.Json.Serialization;

namespace middleware.Models;

public class Subscription
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int ChannelId { get; set; }

    [JsonPropertyName("res-type")]
    public string ResType => "subscription";

    [JsonPropertyName("resource-name")]
    public string ResourceName { get; set; } = "";

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = "";

    [JsonPropertyName("evt")]
    public int Evt { get; set; }

    [JsonPropertyName("dt-creation")]
    public string DtCreation { get; set; } = "";

    [JsonPropertyName("dt-expiration")]
    public string DtExpiration { get; set; } = "";
}
