namespace WaterValve;

using System.Text.Json;
using System.Text.Json.Serialization;

public class NotificationPayload
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = "";

    [JsonPropertyName("resource")]
    public DataRecordResource? Resource { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = "";
}

public class DataRecordResource
{
    [JsonPropertyName("res-type")]
    public string ResType { get; set; } = "";

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

public static class NotificationHandler
{
    public static async Task HandleAsync(HttpContext ctx,
                                         ValveService valve,
                                         XmlNotificationStorage storage)
    {
        using var reader = new StreamReader(ctx.Request.Body);
        var body = await reader.ReadToEndAsync();

        NotificationPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<NotificationPayload>(body);
        }
        catch
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsync("Invalid JSON");
            return;
        }

        if (payload?.Resource == null)
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsync("Missing resource");
            return;
        }

        Console.WriteLine($"Notification: event={payload.Event}, " +
                          $"content={payload.Resource.Content}");

        await storage.SaveAsync(payload);

        if (payload.Event == "creation" &&
            int.TryParse(payload.Resource.Content, out int humidity))
        {
            valve.ProcessHumidity(humidity);
        }

        ctx.Response.StatusCode = 200;
        await ctx.Response.WriteAsync("OK");
    }
}