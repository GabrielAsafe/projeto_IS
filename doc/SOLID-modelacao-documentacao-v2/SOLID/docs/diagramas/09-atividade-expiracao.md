# 09 — Expiração e cascatas

**Estado: [PROPOSTA], não implementação.**

```mermaid
flowchart TD
    Wake([BackgroundService / PeriodicTimer]) --> Find[Consultar records/subscriptions expirados]
    Find --> Any{Ha recursos com expiry anterior a agora?}
    Any -->|Nao| Wait([Aguardar ciclo seguinte])
    Any -->|Sim| Remove[Remover recursos numa transacao]
    Remove --> Type{Era data-record?}
    Type -->|Sim| Policy{Politica Q-05 validada?}
    Type -->|Nao| Wait
    Policy -->|Remocao automatica notifica| Event[Publicar evento deletion pos commit]
    Policy -->|Sem obrigacao confirmada| Record[Registar decisao + testar]
    Event --> Wait
    Record --> Wait
```

**[REQUISITO]** expiração automática de data-record/subscription; **[PROPOSTA]** worker dentro do middleware. O loop é de manutenção da BD, não detecção de medições na Valve. Remoção de records por expiração, por DELETE direto ou por cascata deve ter política de eventos definida em `19-politica-nomes-e-cascatas.md`.
