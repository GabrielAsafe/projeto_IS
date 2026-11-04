
using MQTTnet;
using MQTTnet.Protocol;

namespace middleware.Services;

public class MqttService
{
    public async Task PublishAsync(
        string brokerEndpoint,
        string topic,
        string payload,
        CancellationToken cancellationToken = default)
    {
        // 1. Interpretar o endereço do broker
        if (!Uri.TryCreate(brokerEndpoint,UriKind.Absolute,out var broker)
            || broker.Scheme != "mqtt" || string.IsNullOrWhiteSpace(broker.Host))
        {
            throw new ArgumentException(
                "Invalid MQTT broker endpoint"
            );
        }

        // 2. Criar o cliente MQTT
        var factory = new MqttClientFactory();
        using var client = factory.CreateMqttClient();

        int port = broker.Port > 0 ? broker.Port : 1883;

        // 3. Configurar a ligação
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker.Host, port)
            .Build();

        await client.ConnectAsync(options, cancellationToken);
        //daqui para cima é boilerplate
        try
        {
            // 4. Construir a mensagem
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(
                    MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            // 5. Publicar no broker
            await client.PublishAsync(
                message,
                cancellationToken
            );
        }
        finally
        {
            // 6. Desligar a ligação
            if (client.IsConnected)
                await client.DisconnectAsync();
        }
    }
}
