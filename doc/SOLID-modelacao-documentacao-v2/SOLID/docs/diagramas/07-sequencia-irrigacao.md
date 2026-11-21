# 07 — Comunicação event-driven na irrigação

**Estado: [PROPOSTA], não implementação.**

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Valve as Water Valve App
    participant Broker as MQTT Broker (quando usado)
    participant API as SOLID Middleware
    participant Sensor as Moisture Sensor App
    participant XML as Ficheiro XML / XSD
    User->>Valve: Definir limiar T
    Valve->>API: POST subscription (evt=1)
    API-->>Valve: Recurso criado
    loop A cada segundo no SENSOR
      Sensor->>API: POST data-record humidity H (0..100)
      API-->>Sensor: 201 JSON (nome atribuido)
      alt entrega HTTP
        API-->>Valve: HTTP POST NotificationDto
      else entrega MQTT
        API->>Broker: publish para topico do canal
        Broker-->>Valve: mensagem de evento
      end
      Valve->>XML: Acrescentar notificacao; validar XML com XSD
      alt H abaixo de T
        Valve->>Valve: Estado ON
      else H maior ou igual a T
        Valve->>Valve: Estado OFF
      end
    end
    Note over Valve,Sensor: Sem polling da valvula nem ligacao sensor-valvula
```

Dois percursos alternativos de **entrega** são apresentados, não dois envios obrigatórios por medição para a mesma subscrição. Na prática, a Valve usa o transporte configurado. Se a notificação não trouxer informação suficiente, um GET **disparado pela notificação** é permitido como decisão de integração; não é polling. O XML/XSD é produzido/validado pela Valve.
