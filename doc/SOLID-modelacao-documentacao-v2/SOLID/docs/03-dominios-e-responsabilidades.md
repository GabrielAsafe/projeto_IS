# 03 — Domínios e limites de responsabilidade

Os seguintes **módulos lógicos** são uma proposta técnica; não implicam microserviços independentes.

| Módulo | Responsabilidade | Dados próprios |
|---|---|---|
| Resource Registry | CRUD de `application`, `channel`, `data-record`, `subscription`; nomes e hierarquia | Quatro recursos |
| Discovery | Resolver escopo, percorrer hierarquia e devolver paths por tipo | Leitura, sem entidade própria |
| Subscription & Notifications | Interpretar `evt`, gerir endpoints e emitir notificações HTTP/MQTT | Subscription; estado de envio opcional interno |
| Expiration | Identificar e remover `data-record`/`subscription` vencidos | Datas de expiração e eventuais logs internos |
| Persistence | SQLite, índices, integridade, transações | Quatro tabelas, FKs |
| REST Interface | Resolver URI, validar DTO, códigos HTTP, contrato Swagger/OpenAPI | Nenhum dado de domínio |
| Moisture Sensor Simulator | Produzir amostra de 0–100% por segundo e publicar via REST | Estado transitório do sensor |
| Water Valve Simulator | Consumir notificação, obter medição quando necessário, aplicar limiar, guardar XML e validar XSD | Limiar, estado ON/OFF, histórico XML |

## Dependências permitidas (propostas)

- Camada HTTP → serviços de aplicação → repositórios/domínio.
- CRUD de data-record → componente de eventos → transporte HTTP/MQTT (sem acoplamento ao dispositivo simulado).
- Timer/serviço de expiração → repositórios → eventos de remoção **apenas se confirmado** que a expiração deve notificar.
- Sensor e válvula → API pública do SOLID; nenhum acesso direto à BD do middleware.

## Invariantes

1. Uma `Application` pode possuir múltiplos `Channel`.
2. Um `Channel` pode possuir múltiplos `DataRecord` e múltiplas `Subscription`.
3. Uma `Subscription` tem `evt=1` ou `evt=2` conforme §2.4 (apesar da menção anterior a ambos).
4. `subs` é detalhe do encaminhamento REST, não uma tabela/recurso persistido.
5. O nome público só é único no escopo do pai; identificadores internos não saem da infraestrutura.
