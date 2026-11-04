namespace middleware.Services;

using System.Net.Http.Json;
using middleware.Models;
using System.Text.Json;


public class NotificationService
{
    private readonly SolidDatabase database;
    private readonly HttpClient http;
    private readonly ILogger<NotificationService> logger;
    private readonly MqttService mqtt;
  public NotificationService(
    SolidDatabase database,
    HttpClient http,
    MqttService mqtt,
    ILogger<NotificationService> logger)
    {
        this.database = database;
        this.http = http;
        this.mqtt = mqtt;
        this.logger = logger;
    }
    public async Task NotifyAsync(
        int channelId,
        int eventType,
        DataRecord record,
        string resourcePath,
        CancellationToken cancellationToken = default)
    {
        if (eventType != 1 && eventType != 2)
            throw new ArgumentException("Invalid event type");

        // 1. Procurar subscriptions interessadas
        var subscriptions =
            database.GetInterestedSubscriptions(channelId, eventType);

        if (subscriptions.Count == 0)
            return;

        // 2. Construir a notificação
        var notification = new
        {
            @event = eventType == 1 ? "creation" : "deletion",
            resource = record,
            path = resourcePath
        };

        string json = JsonSerializer.Serialize(notification);

        // Retirar o nome do data-record do caminho
        int lastSlash = resourcePath.LastIndexOf('/');

        if (lastSlash <= 0)
            throw new ArgumentException("Invalid resource path");

        string topic = resourcePath[..lastSlash].TrimStart('/');

        // Evitar publicar várias vezes o mesmo evento
        // no mesmo broker/tópico.
        var publishedBrokers =    new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 3. Percorrer as subscriptions
        foreach (var sub in subscriptions)
        {
            string endpoint = sub["endpoint"]?.ToString() ?? "";

            try
            {
                /*
                endpoint — A string que queres validar, por exemplo "https://google.com".
                UriKind.Absolute — Indica que o URI tem de ser absoluto, ou seja, incluir um esquema como https:// ou http://.
                out var uri — Guarda o objeto Uri criado, caso a conversão seja bem-sucedida.
                Uri.TryCreate() — Devolve true se conseguir criar o URI e false caso contrário.
                scheema é http ou https

                caso o middleware deixe de funcionar, um cancellation token não deixa outros serviços aarrados na rede a espera de resposta
                */
                if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
                {
                    // 4. Enviar notificação HTTP
                    using var response = await http.PostAsJsonAsync(
                        uri,
                        notification,
                        cancellationToken
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogWarning(
                            "Notification failed: {StatusCode}",
                            response.StatusCode
                        );
                    }
                }else if (uri != null && uri.Scheme == "mqtt"){
                if (publishedBrokers.Add(uri.AbsoluteUri)){
                        await mqtt.PublishAsync(
                            endpoint,
                            topic,
                            json,
                            cancellationToken
                        );

                        logger.LogInformation(
                            "MQTT notification published to {Topic}",
                            topic
                        );
                    }
                }
                                    
                logger.LogWarning(
                    "Protocol not implemented: {Endpoint}",
                    endpoint
                );
            }
            
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to notify {Endpoint}",
                    endpoint
                );
            }
        }
    }
}