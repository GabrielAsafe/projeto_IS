# Diagrama de estados da válvula

> **Modelo proposto**, não implementação concluída.

```mermaid
stateDiagram-v2
    [*] --> WaitingForEvent
    WaitingForEvent --> ON: humidity < threshold
    WaitingForEvent --> OFF: humidity >= threshold
    ON --> ON: humidity < threshold
    ON --> OFF: humidity >= threshold
    OFF --> ON: humidity < threshold
    OFF --> OFF: humidity >= threshold
    ON --> [*]: application stopped
    OFF --> [*]: application stopped
```

Estados e transições propostos. Uma notificação com payload inválido deve ser registada/assinalada sem fazer a transição para um estado baseado numa medição inexistente; a política exata deve ser decidida na implementação.
