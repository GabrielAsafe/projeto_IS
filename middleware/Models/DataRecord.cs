using System.Text.Json.Serialization;

namespace middleware.Models;

public class DataRecord
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int ChannelId { get; set; }

    [JsonPropertyName("res-type")]
    public string ResType { get; set; } = "data-record";

    [JsonPropertyName("resource-name")]
    public string ResourceName { get; set; } = "";

    [JsonPropertyName("content-type")]
    public string ContentType { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("dt-creation")]
    public string DtCreation { get; set; } = "";

    [JsonPropertyName("dt-expiration")]
    public string DtExpiration { get; set; } = "";
}