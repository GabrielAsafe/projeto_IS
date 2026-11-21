# 01 — Modelo de domínio (sem serviços)

**Estado: [PROPOSTA], não implementação.**

```mermaid
classDiagram
    direction TB
    class Application {
        +string ResourceName
        +string ResourceDesc
        +DateTime CreatedAt
    }
    class Channel {
        +string ResourceName
        +string ResourceDesc
        +DateTime CreatedAt
    }
    class DataRecord {
        +string ResourceName
        +string ContentType
        +string Content
        +DateTime CreatedAt
        +DateTime? ExpiresAt
    }
    class Subscription {
        +string ResourceName
        +int EventType
        +string Endpoint
        +DateTime CreatedAt
        +DateTime? ExpiresAt
    }
    Application "1" *-- "0..*" Channel : channels
    Channel "1" *-- "0..*" DataRecord : records
    Channel "1" *-- "0..*" Subscription : subscriptions
```

**Origem:** §2 do enunciado. Os quatro tipos têm nomes públicos exatos `application`, `channel`, `data-record` e `subscription`. Aqui só aparecem **entidades de domínio**, não Controllers/Services/DTOs/IDs SQLite. Os nomes PascalCase são internos ao código; `res-type` é propriedade do contrato JSON e não exige uma coluna SQL por entidade.

**Cardinalidade:** foi modelado `0..*` em canais para permitir a criação atómica de uma application ainda sem canais, enquanto o enunciado descreve o estado desejado como 1..N. Esta tensão está registada em Q-18; não esconder o intervalo de configuração.

**Composição:** sugere pertença hierárquica forte, mas **não decide** sozinha a política de cascade delete nem de notificações. Consultar `19-politica-nomes-e-cascatas.md`.
