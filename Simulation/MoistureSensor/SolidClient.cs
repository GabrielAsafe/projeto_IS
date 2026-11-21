namespace MoistureSensor;

using System.Text;
using System.Text.Json;

public class SolidClient
{
    private readonly HttpClient _http;
    private readonly string _base;
    private const string AppName = "moisture-app";
    private const string ChannelName = "moisture-channel";

    public SolidClient(string middlewareUrl)
    {
        _http = new HttpClient();
        _base = middlewareUrl.TrimEnd('/');
    }

    public async Task EnsureResourcesExistAsync()
    {
        await PostIfNotExists(
            $"{_base}/api/solid",
            new { resType = "application", resourceName = AppName, resourceDesc = "Moisture sensor app" }
        );
        await PostIfNotExists(
            $"{_base}/api/solid/{AppName}",
            new { resType = "channel", resourceName = ChannelName, resourceDesc = "Humidity channel" }
        );
    }

    private async Task PostIfNotExists(string url, object body)
    {
        var check = await _http.GetAsync(url.Replace("/api/solid", $"/api/solid/{body}"));
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = new KebabCaseNamingPolicy()
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _http.PostAsync(url, content); // virheet ignoroidaan
    }

    public async Task PublishHumidityAsync(int humidity)
    {
        var url = $"{_base}/api/solid/{AppName}/{ChannelName}";
        var body = new
        {
            resType = "data-record",
            contentType = "text/plain",
            content = humidity.ToString(),
            dtExpiration = ""
        };
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = new KebabCaseNamingPolicy()
        });
        var response = await _http.PostAsync(url,
            new StringContent(json, Encoding.UTF8, "application/json"));
        Console.WriteLine($"Sent humidity={humidity}, status={response.StatusCode}");
    }
}

public class KebabCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
                sb.Append('-');
            sb.Append(char.ToLower(name[i]));
        }
        return sb.ToString();
    }
}