using System.Text.Json.Serialization;

namespace middleware.Models;

public class Channel
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int ApplicationId { get; set; }

    [JsonPropertyName("res-type")]
    public string ResType { get; set; } = "channel";

    [JsonPropertyName("resource-name")]
    public string ResourceName { get; set; } = "";

    [JsonPropertyName("resource-desc")]
    public string ResourceDesc { get; set; } = "";

    [JsonPropertyName("dt-creation")]
    public string DtCreation { get; set; } = "";
}