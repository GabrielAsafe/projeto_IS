O teu `MqttService` está bem encaminhado. Já tens código para ligar a um broker MQTT, subscrever tópicos, publicar mensagens e desligar a ligação.

Mas há um detalhe importante para o projeto SOLID: o middleware precisa principalmente de publicar notificações MQTT, não de as consumir.

O teu código atual faz as duas coisas:

| Método            | Função                                     | Necessário no middleware?         |
| ----------------- | ------------------------------------------ | --------------------------------- |
| `Connect()`       | Liga ao broker MQTT                        | Sim                               |
| `Subscribe()`     | Subscreve um tópico para receber mensagens | Não para o envio das notificações |
| `Publish()`       | Publica uma mensagem num tópico            | Sim                               |
| `Disconnect()`    | Desliga do broker                          | Sim                               |
| `MessageReceived` | Trata mensagens recebidas                  | Útil para um consumidor de testes |

No projeto final, quem vai subscrever o tópico é uma aplicação externa, como a válvula de rega.

## 1. Como deve funcionar o MQTT no SOLID?

DataRecord criado ou eliminado

Evento gerado pela API ou pelo ExpirationService

NotificationService

Encontra subscriptions com evt=1 ou evt=2

MqttService

Publica a notificação no tópico do canal

Broker MQTT — localhost:1883

Encaminha mensagens para os clientes subscritos

Aplicação da válvula

Subscreve o tópico e recebe a notificação

De acordo com o enunciado, uma subscription MQTT deve guardar no campo `endpoint` o endereço do broker. O tópico é calculado a partir do caminho do canal.

Por exemplo:

```
endpoint = mqtt://localhost:1883

topic = api/solid/app1/channel1
```

O problema principal do teu código atual é ter o broker fixo em `localhost:1883`. Se uma subscription indicar outro broker, o middleware tem de conseguir utilizar esse endereço.

## 2. Como adaptaria o teu MqttService

Para a tua responsabilidade, simplificaria a classe para ter um método que recebe o broker, o tópico e a mensagem.

Esta versão utiliza a API do MQTTnet documentada nos exemplos oficiais.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://github.com\&sz=32)

GitHub+1



### Services/MqttService.cs

```

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
        if (!Uri.TryCreate(
                brokerEndpoint,
                UriKind.Absolute,
                out var broker)
            || broker.Scheme != "mqtt"
            || string.IsNullOrWhiteSpace(broker.Host))
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

```

Esta versão suporta brokers `mqtt://` por TCP sem TLS. Para suportar `mqtts://`, será necessário configurar TLS separadamente.

Para já, cada publicação cria uma ligação e fecha-a no final. É simples para testes; quando o sensor publicar a cada segundo, podemos otimizar reutilizando ligações.

## 3. Integrar com o teu NotificationService

Agora precisas de fazer com que o `NotificationService` escolha entre HTTP e MQTT.

Acrescenta a dependência:

```
private readonly MqttService mqtt;
```

E altera o construtor:

```
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
```

Depois, no método `NotifyAsync()`, antes do `foreach` das subscriptions, prepara o tópico e a mensagem:

```
using System.Text.Json;
```

```
string json = JsonSerializer.Serialize(notification);

// Retirar o nome do data-record do caminho
int lastSlash = resourcePath.LastIndexOf('/');

if (lastSlash <= 0)
    throw new ArgumentException("Invalid resource path");

string topic = resourcePath[..lastSlash].TrimStart('/');

// Evitar publicar várias vezes o mesmo evento
// no mesmo broker/tópico.
var publishedBrokers =
    new HashSet<string>(StringComparer.OrdinalIgnoreCase);
```

E acrescenta o caso MQTT ao `foreach` que já tens:

```
else if (uri != null && uri.Scheme == "mqtt")
{
    if (publishedBrokers.Add(uri.AbsoluteUri))
    {
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
```

Este bloco deve ficar dentro do teu `try`, antes do `else` que regista protocolos não suportados.

A verificação `publishedBrokers` evita uma situação importante: se existirem duas subscriptions MQTT para o mesmo broker, canal e tipo de evento, não queres publicar duas mensagens idênticas, porque os consumidores do tópico receberiam duplicados.

No teu `Program.cs`, regista o serviço:

```
builder.Services.AddSingleton<MqttService>();
```

Mantém os registos existentes de `NotificationService`, `SolidDatabase` e `ExpirationService`.

## 4. Como testar MQTT?

Primeiro, precisas de ter um broker MQTT em execução. Podes utilizar o Mosquitto, caso esteja instalado.

Terminal 1 — Broker

```
mosquitto -v
```

Terminal 2 — Consumidor MQTT

```
mosquitto_sub -h localhost -p 1883 \
  -t 'api/solid/app1/channel1' -v
```

Terminal 3 — Middleware SOLID

```
dotnet run --project middleware/
```

Se já tiveres um broker na porta 1883, não precisas de iniciar outro.

### Criar uma subscription MQTT no SQLite

Na raiz do projeto:

```
sqlite3 Data/database.db
```

Executa:

```
INSERT INTO subscriptions (
    channel_id,
    resource_name,
    evt,
    endpoint,
    created_at,
    expires_at
)
SELECT
    ch.id,
    'sub-mqtt-create',
    1,
    'mqtt://localhost:1883',
    strftime('%Y-%m-%dT%H:%M:%S', 'now'),
    NULL
FROM channels ch
JOIN applications app
    ON ch.application_id = app.id
WHERE app.resource_name = 'app1'
  AND ch.resource_name = 'channel1';
```

Isto cria uma subscription interessada na criação de `data-records`.

Depois podes executar o endpoint temporário que já tens:

```
curl -X POST http://localhost:8765/dev/test-event
```

Se estiver tudo correto, o consumidor `mosquitto_sub` deverá receber o JSON da notificação no tópico:

`api/solid/app1/channel1`

E, se a tua subscription HTTP `sub1` já tiver expirado, isso não interfere com a subscription MQTT, porque são recursos independentes.

## 5. O que falta na tua parte?

Integração MQTT

1/9

Manter o teste HTTP que já funciona

Adaptar MqttService para receber o broker como parâmetro

Registar MqttService no Program.cs

Integrar MQTT no NotificationService

Criar uma subscription MQTT na base de dados

Iniciar Mosquitto e um consumidor de teste

Confirmar a receção do JSON por MQTT

Testar também evt=2 através de MQTT

Integrar os eventos com o CRUD real da Pessoa 2

O essencial é manteres a separação entre os serviços:

O `NotificationService` decide quem notificar e através de que protocolo. O `MqttService` apenas sabe como publicar uma mensagem num broker MQTT.

Desta forma, quando a Pessoa 2 integrar os eventos reais de criação e eliminação, o teu `NotificationService` poderá distribuir automaticamente as notificações tanto por HTTP como por MQTT, sem ser necessário alterar o CRUD.