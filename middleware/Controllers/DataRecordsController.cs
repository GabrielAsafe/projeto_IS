using Microsoft.AspNetCore.Mvc;
using middleware.DTOs;
using middleware.Models;
using middleware.Services;
using Microsoft.Data.Sqlite;
using System.Text.Json;
namespace middleware.Controllers;

[ApiController]
[Route("api/solid")]
public class DataRecordsController : ControllerBase
{
    private readonly SolidDatabase database;
    private readonly NotificationService notification;

    public DataRecordsController(SolidDatabase database, NotificationService notificationService)
    {
        this.database = database;
        this.notification = notificationService;
    }

    // GET /api/solid/app1/channel1/dr1
    [HttpGet("{appName}/{channelName}/{recordName}")]
    public IActionResult Get(
        string appName,
        string channelName,
        string recordName)
    {
        var record = database.GetDataRecord(
            appName, channelName, recordName);

        if (record == null)
            return NotFound();

        return Ok(record);
    }



    // Dentro do DataRecordsController

    [HttpPost("{appName}/{channelName}")]
    public async Task<IActionResult> Create(
        string appName,
        string channelName,
        [FromBody] JsonElement request)
    {
        // 1. Validar a existência do canal
        var channel = database.GetChannel(appName, channelName);

        if (channel == null)
            return NotFound();

        // 2. Obter o tipo de recurso
        if (request.ValueKind != JsonValueKind.Object)
            return BadRequest("Request must be a JSON object");

        if (!request.TryGetProperty("res-type", out var typeElement)
            || typeElement.ValueKind != JsonValueKind.String)
        {
            return BadRequest("Invalid res-type");
        }

        string? resourceType = typeElement.GetString();

        // 3. Encaminhar conforme o tipo
        switch (resourceType)
        {
            case "data-record":
                {
                    // 1. Desserializar DataRecordRequest
                    DataRecordRequest? re;

                    try
                    {
                        re = request.Deserialize<DataRecordRequest>();
                    }
                    catch (JsonException)
                    {
                        return BadRequest("Invalid DataRecordRequest");
                    }

                    if (re == null)
                        return BadRequest("Invalid request");

                    // 2. Validar content-type
                    string contentType = re.ContentType?.Trim().ToLowerInvariant() ?? "";

                    if (contentType != "application/json" &&
                         contentType != "application/xml" &&
                         contentType != "text/plain")
                    {
                        return BadRequest("Invalid content-type");
                    }

                    // 3. Validar content
                    if (re.Content == null)
                        return BadRequest("Content is required");

                    string content = re.Content;

                    // 4. Validar dt-expiration
                    string? expiration = null;

                    if (!string.IsNullOrWhiteSpace(re.DtExpiration))
                    {
                        if (!DateTimeOffset.TryParse(
                                re.DtExpiration,
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.AssumeUniversal,
                                out var date))
                        {
                            return BadRequest("Invalid dt-expiration");
                        }

                        if (date <= DateTimeOffset.UtcNow)
                            return BadRequest("Expiration must be in the future");

                        expiration = date.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss'Z'");
                    }

                    // 5. Gerar resource-name único
                    string resName = string.IsNullOrWhiteSpace(re.ResourceName)
                        ? $"dr-{Guid.NewGuid():N}"
                        : re.ResourceName.Trim();

                    // 6. Inserir na SQLite
                    DataRecord? record;

                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        try
                        {
                            record = database.InsertDataRecord(
                                 appName,
                                 channelName,
                                 resName,
                                 contentType,
                                 content,
                                 expiration
                             );

                            // 7. Obter DataRecord criado
                            if (record == null)
                                return NotFound("Channel not found");

                            // 8. Construir resourcePath
                            string resourcePath =
                                $"/api/solid/{Uri.EscapeDataString(appName)}" +
                                $"/{Uri.EscapeDataString(channelName)}" +
                                $"/{Uri.EscapeDataString(record.ResourceName)}";

                            // 9. Notificar subscritores
                            await notification.NotifyAsync(
                                record.ChannelId,
                                1,
                                record,
                                resourcePath
                            );

                            // 10. Devolver 201 Created
                            return Created(resourcePath, record);
                        }
                        catch (SqliteException ex) when (
                            ex.SqliteErrorCode == 19 &&
                            ex.SqliteExtendedErrorCode == 2067)
                        {
                            // Nome duplicado: gerar outro e tentar novamente
                            resName = $"dr-{Guid.NewGuid():N}";

                        }
                    } //resolver nomes repetidos

                    return StatusCode(500, "Unable to generate a unique resource name");
                }

            case "subscription":
                {
                    SubscriptionsRequest? re;

                    try
                    {
                        re = request.Deserialize<SubscriptionsRequest>();
                    }
                    catch (JsonException)
                    {
                        return BadRequest("Invalid SubscriptionRequest");
                    }

                    if (re == null)
                        return BadRequest("Invalid request");

                    // 1. Validar evt
                    if (re.Evt != 1 && re.Evt != 2)
                        return BadRequest("evt must be 1 or 2");

                    // 2. Validar endpoint
                    string endpoint = re.Endpoint?.Trim() ?? "";

                    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
                        || string.IsNullOrWhiteSpace(uri.Host)
                        || (uri.Scheme != "http" &&
                            uri.Scheme != "https" &&
                            uri.Scheme != "mqtt"))
                    {
                        return BadRequest("Invalid endpoint");
                    }

                    // 3. Validar dt-expiration
                    string? expiration = null;

                    if (!string.IsNullOrWhiteSpace(re.DtExpiration))
                    {
                        if (!DateTimeOffset.TryParse(
                            re.DtExpiration,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AssumeUniversal,
                            out var date))
                        {
                            return BadRequest("Invalid dt-expiration");
                        }

                        if (date <= DateTimeOffset.UtcNow)
                            return BadRequest("Expiration must be in the future");

                        expiration = date.UtcDateTime.ToString(
                            "yyyy-MM-ddTHH:mm:ss'Z'");
                    }

                    // 4. Gerar resource-name
                    string resName = string.IsNullOrWhiteSpace(re.ResourceName)
                        ? $"sub-{Guid.NewGuid():N}"
                        : re.ResourceName.Trim();

                    // 5. Inserir na SQLite
                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        try
                        {
                            var subscription = database.InsertSubscription(
                                appName,
                                channelName,
                                resName,
                                re.Evt.Value,
                                endpoint,
                                expiration
                            );

                            if (subscription == null)
                                return NotFound("Channel not found");

                            // 6. Construir resourcePath
                            string resourcePath =
                                $"/api/solid/{Uri.EscapeDataString(appName)}" +
                                $"/{Uri.EscapeDataString(channelName)}" +
                                $"/subs/{Uri.EscapeDataString(subscription.ResourceName)}";

                            // 7. Devolver 201 Created
                            return Created(resourcePath, subscription);
                        }
                        catch (SqliteException ex) when (
                            ex.SqliteErrorCode == 19 &&
                            ex.SqliteExtendedErrorCode == 2067)
                        {
                            resName = $"sub-{Guid.NewGuid():N}";
                        }
                    }

                    return StatusCode(500,
                        "Unable to generate a unique resource name");
                }


            default:
                return BadRequest("Invalid res-type");
        }
    }



    // DELETE /api/solid/app1/channel1/dr1
    [HttpDelete("{appName}/{channelName}/{recordName}")]
    public async Task<IActionResult> Delete(
        string appName,
        string channelName,
        string recordName)
    {
        // 1. Obter o record antes de eliminar
        var record = database.GetDataRecord(
            appName, channelName, recordName);

        if (record == null)
            return NotFound();

        // 2. Construir o caminho completo
        string resourcePath =
            $"/api/solid/{Uri.EscapeDataString(appName)}" +
            $"/{Uri.EscapeDataString(channelName)}" +
            $"/{Uri.EscapeDataString(recordName)}";

        // 3. Eliminar da base de dados
        bool deleted = database.DeleteDataRecord(
            appName, channelName, recordName);

        if (!deleted)
            return NotFound();

        // 4. Notificar a Pessoa 3
        await notification.NotifyAsync(
            record.ChannelId,
            2,
            record,
            resourcePath
        );

        return NoContent();
    }




}