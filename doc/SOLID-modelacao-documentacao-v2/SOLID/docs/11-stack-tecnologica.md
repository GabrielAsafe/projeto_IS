# 11 — Stack tecnológica: critérios e limites (v2)

**[REQUISITO]** implementar em .NET 10 com persistência SQLite e API REST JSON, porta 8765, Swagger/OpenAPI, notificações HTTP e MQTT. **[REQUISITO]** o §6 limita tecnologias/frameworks/metodologias às trabalhadas nas aulas práticas. Todas as bibliotecas abaixo são **candidatas**, não confirmações.

| Camada | Candidato A | Candidato B | Critério |
|---|---|---|---|
| API | ASP.NET Core Controllers | ASP.NET Core Minimal APIs | Clareza rota hierárquica, OpenAPI, práticas |
| Persistência | EF Core SQLite | Microsoft.Data.Sqlite / ADO.NET | Mapeamento, controlo sobre triggers, aulas |
| MQTT | Biblioteca MQTTnet **se autorizada** | Cliente MQTT ensinado nas aulas | Publisher, URI broker, tópicos, QoS, broker prática |
| Serviço interno TTL | `BackgroundService` + `PeriodicTimer` | implementação direta de `IHostedService` | Simplicidade, ciclo de vida e testes, aulas |
| Sensor | Console/Worker .NET 10 | UI .NET 10 | publicar a cada segundo, independente |
| Valve | Endpoint HTTP POST | Cliente MQTT subscrito | notificação push sem polling; XML + XSD |
| OpenAPI | APIs de OpenAPI do ASP.NET | Swagger UI adicional | Esquemas completos + UI se exigida |

**Correção conceptual:** `BackgroundService` **é uma implementação de** `IHostedService` (não são equivalentes semânticos), e `PeriodicTimer` controla frequência de execução do worker. `MQTTnet` é biblioteca de integração MQTT; **não substitui obrigatoriamente** um broker configurado/executável. Confirmação docente necessária.

## Estrutura lógica de solução (opcional)

- `Solid.Api`: rotas, JSON/DTOs, status e OpenAPI.
- `Solid.Domain`: as quatro entidades e invariantes.
- `Solid.Application`: criação/alteração/remoção, discovery, notificações (coordenação).
- `Solid.Infrastructure`: SQLite, HTTP/MQTT, worker de expiração.
- `Solid.SensorSimulator` e `Solid.ValveSimulator`: **aplicações independentes**.
- `Solid.Tests`: testes do contrato e dos fluxos.

A organização por vários projetos C# pode ser simplificada para a disciplina: a regra é **separação lógica**, não multiplicar assemblies.

## Technical spikes recomendados

1. Resolver `res-type` num mesmo POST do canal e documentá-lo no OpenAPI.
2. Verificar rotas `/subs` e `solid-discovery` com os exemplos oficiais.
3. Confirmar SQLite FKs, triggers de nomes, transações e modelo de expiração.
4. Experienciar HTTP POST e MQTT (broker/topic e Q-08).
5. Verificar XML/XSD e fluxo evento→GET opcional→decisão da Valve.

Referências oficiais do runtime a consultar **após** confirmação da tecnologia autorizada:
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-10.0
- https://learn.microsoft.com/en-us/ef/core/providers/sqlite/
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0
