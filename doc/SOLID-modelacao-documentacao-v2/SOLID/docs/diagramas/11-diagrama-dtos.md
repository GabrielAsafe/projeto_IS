# 11 — DTOs, res-type e mapeamento JSON

**Estado: [PROPOSTA], não implementação.**

```mermaid
classDiagram
    direction TB
    class CreateResourceEnvelope {
        +string ResourceType
    }
    class CreateApplicationDto {
        +string ResourceType
        +string? ResourceName
        +string ResourceDesc
    }
    class CreateChannelDto {
        +string ResourceType
        +string? ResourceName
        +string ResourceDesc
    }
    class CreateDataRecordDto {
        +string ResourceType
        +string? ResourceName
        +string ContentType
        +string Content
        +DateTime? Expiration
    }
    class CreateSubscriptionDto {
        +string ResourceType
        +string? ResourceName
        +int EventType
        +string Endpoint
        +DateTime? Expiration
    }
    class ResourceResponseDto {
        +string ResourceType
        +string ResourceName
        +DateTime CreationDate
    }
    class NotificationDto {
        +string Event
        +string ResourcePath
        +DataRecordResponseDto DataRecord
    }
    class DtoResolver {
        +Resolve(envelope, json)
    }
    class DtoMapper {
        +ToResponse(domainObject)
    }
    DtoResolver --> CreateResourceEnvelope
    DtoResolver ..> CreateApplicationDto
    DtoResolver ..> CreateChannelDto
    DtoResolver ..> CreateDataRecordDto
    DtoResolver ..> CreateSubscriptionDto
    DtoMapper --> ResourceResponseDto
    NotificationDto --> ResourceResponseDto : embeds record projection
```

Este diagrama apresenta tipos *conceptuais*, não herança obrigatória. `CreateResourceEnvelope` serve apenas para ler `res-type`; o resolver deve validar tipo e **caminho do pai**. Os DTOs específicos têm campos diferentes. `ResourceResponseDto` simboliza uma **família** de DTOs de resposta que inclui todos os campos oficiais de cada recurso; não deve ser implementado como resposta incompleta. `NotificationDto` é [PROPOSTA], pois o enunciado não fixa as chaves do evento. Ver exemplos exatos em `../18-contratos-dto-e-mapeamento.md`.
