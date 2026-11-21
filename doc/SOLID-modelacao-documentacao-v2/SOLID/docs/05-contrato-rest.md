# 05 — REST API e operações (v2)

**[REQUISITO]** Base `http://<host>:8765/api/solid`. JSON em requests/responses. A tabela identifica **propostas de rotas REST consistentes** com os exemplos do enunciado; operações de PUT não aparecem exemplificadas e deverão ser validadas.

## Rotas CRUD

| Tipo | Criar | Consultar | Atualizar | Remover |
|---|---|---|---|---|
| `application` | `POST /api/solid` | `GET /api/solid/{app}` | `PUT /api/solid/{app}` | `DELETE /api/solid/{app}` |
| `channel` | `POST /api/solid/{app}` | `GET /api/solid/{app}/{channel}` | `PUT /api/solid/{app}/{channel}` | `DELETE /api/solid/{app}/{channel}` |
| `data-record` | `POST /api/solid/{app}/{channel}` | `GET /api/solid/{app}/{channel}/{record}` | **não suportado** | `DELETE /api/solid/{app}/{channel}/{record}` |
| `subscription` | `POST /api/solid/{app}/{channel}` | `GET /api/solid/{app}/{channel}/subs/{subscription}` | **não suportado** | `DELETE /api/solid/{app}/{channel}/subs/{subscription}` |

As duas operações `POST` para o mesmo canal usam o campo obrigatório `res-type` no JSON para diferenciar a criação de `data-record` versus `subscription`. Não criar `POST .../subs` sem confirmação: o enunciado diz expressamente que a criação identifica o **canal pai**.

## Exemplos de payloads públicos (campos exatos)

### Criar application

```http
POST /api/solid
Content-Type: application/json
```
```json
{"res-type":"application","resource-name":"app1","resource-desc":"IoT application"}
```

### Criar channel

```http
POST /api/solid/app1
Content-Type: application/json
```
```json
{"res-type":"channel","resource-name":"channel1","resource-desc":"Measurements"}
```

### Criar data-record

```http
POST /api/solid/app1/channel1
Content-Type: application/json
```
```json
{"res-type":"data-record","resource-name":"dr1","content-type":"application/json","content":"{\"humidity\":45}","dt-expiration":null}
```

`content` é uma string, mesmo quando `content-type` indica JSON. `dt-creation` é acrescentado automaticamente pelo servidor na resposta. O valor nulo exemplifica ausência de expiração, sujeito a confirmação do formato de representação.

### Criar subscription HTTP

```http
POST /api/solid/app1/channel1
Content-Type: application/json
```
```json
{"res-type":"subscription","resource-name":"sub1","evt":1,"endpoint":"http://localhost:9090/notifications","dt-expiration":null}
```

### Descoberta de recursos

```http
GET /api/solid/app1
solid-discovery: data-record
```
```json
["/api/solid/app1/channel1/dr1"]
```

```http
GET /api/solid/app1
solid-discovery: subscription
```
```json
["/api/solid/app1/channel1/subs/sub1"]
```

## Códigos HTTP sugeridos (não completamente especificados)

- Criação: `201 Created` + corpo com a representação completa e nome efetivamente atribuído.
- Consulta/atualização/descoberta: `200 OK` + JSON (a descoberta impossível devolve `[]`).
- Remoção: `204 No Content` (a menos que o docente exija representação; o requisito de «representação completa» refere explicitamente criar/atualizar).
- Nome/pai inexistente: `404 Not Found`; payload inválido: `400 Bad Request`.
- Atualização de imutáveis: `405 Method Not Allowed` ou `400`, decisão a validar e documentar no OpenAPI.

A REST API nunca devolve IDs internos SQLite, e não implementa um `GET /api/solid` que devolva todos os recursos sem o cabeçalho de descoberta.

## Leitura arquitetural v2

O mesmo `POST /api/solid/{app}/{channel}` aceita `data-record` e `subscription`, diferindo por `res-type` **obrigatório**. Não é um DTO único com todas as propriedades; na proposta, a camada REST seleciona DTO especializado por tipo depois de ler um envelope, valida o pai e transforma para domínio. O JSON e o DTO não são a mesma entidade conceptual. `dt-creation` é gerado no servidor; não persistir a coluna `res-type` é uma implementação possível. Contratos pormenorizados em `18-contratos-dto-e-mapeamento.md`.

Para remoções por expiração/cascata, consultar `19-politica-nomes-e-cascatas.md`; para notas de interpretação do professor, `13-duvidas-e-decisoes.md`.
