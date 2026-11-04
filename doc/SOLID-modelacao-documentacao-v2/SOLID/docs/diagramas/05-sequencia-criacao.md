# 05 — Criação de data-record e notificação

**Estado: [PROPOSTA], não implementação.**

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant API as API + DTO dispatch
    participant App as ResourceApplicationService
    participant Repo as Repositories
    participant DB as SQLite
    participant Notif as NotificationCoordinator
    participant Adapter as HTTP / MQTT adapters
    Client->>API: POST /api/solid/app1/channel1 (res-type=data-record)
    API->>API: Validar res-type + CreateDataRecordDto
    API->>App: CreateDataRecord(channel, dto)
    App->>Repo: Verificar pai, nome e restricoes
    Repo->>DB: INSERT + COMMIT
    DB-->>Repo: Record persistido
    Repo-->>App: Entidade persistida
    App-->>API: ResourceResponseDto
    API-->>Client: 201 + representacao completa
    App-->>Notif: RecordCreated (apos COMMIT)
    Notif->>Repo: Consultar subscriptions evt=1
    Repo-->>Notif: Destinatarios do canal
    loop Para cada destinatario
      Notif->>Adapter: Enviar NotificationDto
    end
    Note over App,Adapter: Sem garantia duravel de entrega na proposta minima
```

O `res-type` é um **discriminador** obrigatório no corpo JSON; não é sinal de que exista uma só entidade ou uma única classe DTO. O servidor escolhe o DTO específico a partir do envelope. A persistência precede a emissão do evento para evitar notificar dados que ainda não foram guardados. O momento da resposta HTTP face ao envio assíncrono deve ser definido/testado; aqui o envio é apresentado logicamente depois do commit, sem promessa de tempo real.
