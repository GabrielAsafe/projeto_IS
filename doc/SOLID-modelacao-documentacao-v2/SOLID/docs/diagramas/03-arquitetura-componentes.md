# 03 — Arquitetura de componentes proposta

**Estado: [PROPOSTA], não implementação.**

```mermaid
flowchart TB
    Sensor[Moisture Sensor App] -->|REST POST 1/s| REST
    Other[Outros clientes] -->|HTTP JSON| REST
    subgraph MW[Middleware SOLID .NET 10 - um processo modular]
      REST[Camada API - rotas /api/solid, DTOs, Swagger]
      Resource[ResourceApplicationService]
      Discovery[DiscoveryService]
      Expiration[ExpirationWorker]
      Outbox[NotificationCoordinator - apos persistencia]
      Repo[Interfaces repositório / unidade de trabalho]
      Persist[SQLite repositories]
      HTTP[HTTP notification adapter]
      MQTT[MQTT notification adapter]
      REST --> Resource
      REST --> Discovery
      Resource --> Repo
      Discovery --> Repo
      Expiration --> Resource
      Repo --> Persist
      Persist --> DB[(SQLite)]
      Resource -.->|evento pos commit| Outbox
      Outbox --> HTTP
      Outbox --> MQTT
    end
    HTTP -->|HTTP POST| Valve[Water Valve App]
    MQTT -->|publish| Broker[MQTT Broker]
    Broker -->|push| Valve
    Valve --> XML[Registo XML validado com XSD]
    REST --> Swagger[Swagger / OpenAPI]
```

**Separação:** camada REST trata protocolo/DTO; aplicação orquestra casos de uso; domínio contém invariantes; infraestrutura implementa SQLite e transportes. Aplicações Sensor/Valve são **processos independentes** e só se ligam via SOLID. Não são microserviços internos.

**Nota:** a seta de evento pós-commit exprime ordem lógica, não garante entrega confiável. Uma outbox persistente é apenas uma **alternativa opcional**. Contrato e fiabilidade: `../07-notificacoes-e-expiracao.md`.
