# 10 — Serviços e dependências (sem entidades de negócio)

**Estado: [PROPOSTA], não implementação.**

```mermaid
classDiagram
    direction LR
    class SolidResourceController {
        +Post(path, json)
        +Get(path, discoveryHeader)
        +Put(path, json)
        +Delete(path)
    }
    class CreateDtoResolver {
        +Resolve(resType, json)
    }
    class ResourceApplicationService {
        +Create(parentPath, dto)
        +Retrieve(targetPath)
        +Update(targetPath, dto)
        +Delete(targetPath)
    }
    class DiscoveryService {
        +Discover(scopePath, resType)
    }
    class IResourceRepository {
        <<interface>>
        +Find(path)
        +Create(resource)
        +Delete(resource)
        +FindDescendants(scope, type)
    }
    class NotificationCoordinator {
        +OnRecordCreated(record)
        +OnRecordDeleted(record)
    }
    class INotificationTransport {
        <<interface>>
        +Send(subscription, event)
    }
    class HttpNotificationAdapter
    class MqttNotificationAdapter
    class ExpirationWorker {
        +ExecuteAsync()
    }
    SolidResourceController --> CreateDtoResolver
    SolidResourceController --> ResourceApplicationService
    SolidResourceController --> DiscoveryService
    ResourceApplicationService --> IResourceRepository
    DiscoveryService --> IResourceRepository
    ResourceApplicationService ..> NotificationCoordinator
    NotificationCoordinator --> IResourceRepository
    NotificationCoordinator --> INotificationTransport
    HttpNotificationAdapter ..|> INotificationTransport
    MqttNotificationAdapter ..|> INotificationTransport
    ExpirationWorker --> ResourceApplicationService
```

Este é um **diagrama de responsabilidades e dependências**, não de processos. Interfaces/repositórios são [PROPOSTA]; não é necessário implementar este número de classes literalmente. `ResourceApplicationService` valida regras; os adaptadores conhecem HTTP/MQTT mas não a regra de negócio da irrigação. `ExpirationWorker` pode invocar um caso de uso específico de remoção em vez de SQL direto, para evitar bypass às regras/eventos.
