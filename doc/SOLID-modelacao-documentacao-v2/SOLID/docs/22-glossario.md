# 22 — Glossário de termos e siglas

| Termo | Definição no contexto SOLID |
|---|---|
| SOLID | **Service-Oriented Layer for Interoperable Data** (nome da plataforma, não os cinco princípios SOLID OO) |
| `SOL-F01` | `SOL` = projeto; `F` = **Functional** (requisito funcional); `01` = número sequencial; IDs criados pela equipa, não pelo docente |
| `Q-13` | questão nº 13 do registo `13-duvidas-e-decisoes.md`: retry, QoS, ordenação e garantias de entrega |
| API | Application Programming Interface |
| REST | Representational State Transfer |
| CRUD | Create, Read, Update, Delete |
| DTO | Data Transfer Object; estrutura para transportar dados através da fronteira da API |
| `res-type` | campo JSON público que identifica `application`, `channel`, `data-record` ou `subscription`; **não** é um identificador SQL |
| HTTP | Hypertext Transfer Protocol |
| MQTT | protocolo publish/subscribe, historicamente MQ Telemetry Transport |
| QoS | Quality of Service, níveis de garantia do protocolo MQTT |
| JSON | JavaScript Object Notation |
| XML | Extensible Markup Language |
| XSD | XML Schema Definition |
| SQLite | motor de BD relacional embebido exigido para persistência |
| PK / FK | Primary Key / Foreign Key; identificadores internos, não URLs públicos |
| ORM | Object-Relational Mapper (opção tecnológica, não obrigação) |
| `BackgroundService` | classe base .NET para serviços de background; implementa `IHostedService` |
| `PeriodicTimer` | timer .NET proposto para correr o processo de expiração periodicamente |
| Broker | intermediário MQTT entre publishers e subscribers |
| TTL | Time To Live; neste projeto, data `dt-expiration` e política de remoção |
| ADR | Architecture Decision Record; justificação e estado de uma decisão |
| Monólito modular | um processo do middleware com responsabilidades internas separadas |
| OpenAPI | descrição formal do contrato REST; Swagger pode disponibilizar interface de exploração |

**Marcação usada:** `[REQUISITO]` = consta do enunciado; `[PROPOSTA]` = decisão técnica reversível; `[QUESTÃO]` = ambiguidade a confirmar.
