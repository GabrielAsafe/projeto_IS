# 13 — Eliminação de recursos e eventos

**Estado: [PROPOSTA], não implementação.**

```mermaid
flowchart TD
    Call[DELETE publico ou limpeza por expiracao] --> Locate[Identificar recurso pelo nome/caminho]
    Locate --> Child{Tipo de recurso}
    Child -->|data-record| Snapshot[Capturar snapshot + caminho antes do DELETE]
    Child -->|subscription| RemoveSub[Remover subscricao sem evento data-record]
    Child -->|application/channel| Desc{Cascata Q-04 confirmada?}
    Desc -->|Sim| Enumerate[Enumerar records descendentes se politica pedir eventos]
    Desc -->|Nao| Reject[Rejeitar ou aplicar estrategia acordada]
    Snapshot --> Commit[Remover e confirmar transacao]
    RemoveSub --> Commit
    Enumerate --> Commit
    Commit --> Notify{Evento deletion deve ser emitido? Q-05}
    Notify -->|Sim| Dispatch[Enviar evt=2 pos commit com snapshot]
    Notify -->|Nao/por decidir| Log[Registar comportamento e teste]
```

Eliminar um record requer preservar a representação completa **antes** de apagar, porque a notificação tem de incluir o recurso afetado. A existência de `ON DELETE CASCADE` na SQLite não gera automaticamente notificações externas; estas devem ser coordenadas pela aplicação se a política exigir. As questões Q-04/Q-05 continuam explícitas.
