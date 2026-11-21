# 15 — Architecture Decision Records (ADRs) — v2

Os ADRs **não** substituem exigências do enunciado nem validação do professor.

## ADR-001 — Middleware num monólito modular
- **Estado:** proposto.
- **Decisão:** um processo API .NET 10, com módulos/camadas API, Aplicação, Domínio, Infraestrutura.
- **Motivo:** contrato único, hierarquia pequena e SQLite única. Os dois simuladores são processos independentes.
- **Tradeoff:** simplicidade e operações locais vs disciplina de separação de responsabilidades.

## ADR-002 — DTO público vs entidade SQL
- **Estado:** proposto (separação é altamente recomendável).
- **Decisão:** envelope `res-type` obrigatório, DTO específico por tipo; serialização de respostas sem IDs internos.
- **Motivo:** POST genérico no pai, nomes JSON hifenizados obrigatórios, PK/FK privadas.
- **Tradeoff:** mapeamento explícito aumenta código mas garante interoperabilidade.

## ADR-003 — Contrato comum de notificações HTTP/MQTT
- **Estado:** proposto; Q-07 e Q-08 pendentes.
- **Decisão:** JSON com `event`, `resource-path` e `data-record` em ambos os transportes; endpoint MQTT = broker, tópico derivado.
- **Motivo:** reutilização na Valve; cumprir três dados essenciais do enunciado.
- **Tradeoff:** convenção de URI do broker e handling de retries ainda por acordar.

## ADR-004 — Expiração como trabalho interno
- **Estado:** proposto; Q-05 pendente para eventos.
- **Decisão:** `BackgroundService + PeriodicTimer`, armazenamento temporal normalizado, remoção TTL.
- **Motivo:** housekeeping independente de pedidos; não implica polling do sensor pela Valve.
- **Tradeoff:** frequência de limpeza e implicações de notificações.

## ADR-005 — Nomes únicos por pai, incluindo filhos de tipos distintos
- **Estado:** proposto; Q-03 e Q-21 pendentes.
- **Decisão:** manter triggers cruzados e reservar `subs` para evitar ambiguidades de nomes/rotas.
- **Motivo:** leitura estrita da unicidade e path virtual.
- **Tradeoff:** poderá ser necessário relaxar caso se permita reutilização entre tipos.

## ADR-006 — Cascata referencial separada de publicação de eventos
- **Estado:** proposto; Q-04 e Q-05 pendentes.
- **Decisão:** integridade referencial SQLite via cascade; captura explícita de snapshots e publicação pós-commit caso a política exija events de descendants.
- **Motivo:** triggers SQL não enviam HTTP/MQTT e o recurso precisa existir como snapshot numa notification de deletion.
- **Tradeoff:** projeto necessita política clara para cascatas e ordenação dos envios.
