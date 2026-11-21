# 02 — Modelo ER SQLite

**Estado: [PROPOSTA], não implementação.**

```mermaid
erDiagram
    APPLICATIONS ||--o{ CHANNELS : contains
    CHANNELS ||--o{ DATA_RECORDS : contains
    CHANNELS ||--o{ SUBSCRIPTIONS : contains
    APPLICATIONS {
        integer id PK
        string resource_name UK
        string resource_desc
        string created_at
    }
    CHANNELS {
        integer id PK
        integer application_id FK
        string resource_name
        string resource_desc
        string created_at
    }
    DATA_RECORDS {
        integer id PK
        integer channel_id FK
        string resource_name
        string content_type
        string content
        string created_at
        string expires_at
    }
    SUBSCRIPTIONS {
        integer id PK
        integer channel_id FK
        string resource_name
        integer evt
        string endpoint
        string created_at
        string expires_at
    }
```

**Origem:** relações da hierarquia §2. A BD acrescenta PK/FK exclusivamente internas (§2.1). `subs` não existe na BD; é um nó **virtual** da rota. A política provisória v2 proíbe colisões de nomes **entre** `DATA_RECORDS` e `SUBSCRIPTIONS` no mesmo canal, usando triggers (o Mermaid ER não expressa triggers); ver `../../sql/schema.sql`. Os IDs nunca aparecem nos URLs nem no JSON.
