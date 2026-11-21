# 18 — Contratos JSON, DTOs e mapeamento

**Regra [REQUISITO]:** os nomes públicos escritos com hífen devem ser **exatamente** os do enunciado. Os nomes de classes/propriedades C# abaixo são internos e [PROPOSTA]. A documentação v2 não acrescenta campos públicos aos recursos.

## 1. `res-type` como discriminador, não como tabela

Pseudofluxo na API:

```text
POST /api/solid/app1/channel1 + JSON
  ├─ extrair 'res-type' (obrigatório)
  ├─ verificar que o pai é um channel
  ├─ 'data-record'  -> deserializar/validar CreateDataRecordDto
  ├─ 'subscription' -> deserializar/validar CreateSubscriptionDto
  └─ outros         -> 400, tipo incompatível com pai
```

O `res-type` existe **mesmo que usemos vários Controllers**: é obrigatório por contrato. Não precisa de ser coluna SQL se o tipo for inferível da tabela. O JSON funciona como representação de transporte, que **pode** ser convertido num DTO; um JSON não é, tecnicamente, uma classe DTO por si só.

## 2. DTOs de criação / atualização (proposta)

| DTO interno | `res-type` obrigatório | Propriedades JSON do pedido | URI pai |
|---|---|---|---|
| `CreateApplicationDto` | `application` | `res-type`, `resource-name` opcional, `resource-desc` | `/api/solid` |
| `CreateChannelDto` | `channel` | `res-type`, `resource-name` opcional, `resource-desc` | `/api/solid/{app}` |
| `CreateDataRecordDto` | `data-record` | `res-type`, `resource-name` opcional, `content-type`, `content`, `dt-expiration` opcional | `/api/solid/{app}/{ch}` |
| `CreateSubscriptionDto` | `subscription` | `res-type`, `resource-name` opcional, `evt`, `endpoint`, `dt-expiration` opcional | `/api/solid/{app}/{ch}` |
| `UpdateApplicationDto` | `application` | `res-type`, `resource-desc` e eventual renomeação **por confirmar** | URI do recurso |
| `UpdateChannelDto` | `channel` | `res-type`, `resource-desc` e eventual renomeação **por confirmar** | URI do recurso |

`dt-creation` é gerado pelo middleware (não usar valor do cliente como fonte de verdade). Não existe Update DTO para `data-record` ou `subscription` por serem imutáveis.

## 3. Exemplo de envelope de entrada

```json
{
  "res-type": "data-record",
  "resource-name": "dr1",
  "content-type": "application/json",
  "content": "{\"humidity\":45}",
  "dt-expiration": null
}
```

Exemplo de resposta **completa** (inclui propriedades oficiais, sem IDs internos):

```json
{
  "res-type": "data-record",
  "resource-name": "dr1",
  "content-type": "application/json",
  "content": "{\"humidity\":45}",
  "dt-creation": "2026-10-27T09:00:00Z",
  "dt-expiration": null
}
```

`dt-expiration: null` é uma **convenção proposta**; o enunciado permite campo vazio/omitido, sem fixar a serialização de saída. O `content` permanece string embora o seu texto possa ser JSON/XML.

## 4. Mapeamento conceptual

```text
JSON HTTP ⇄ Create*/Update* DTO ⇄ casos de uso ⇄ entidade de domínio ⇄ tabela SQLite
                                               ⇓
                                        ResourceResponseDto ⇄ JSON HTTP
```

Exemplo de anotação (sujeita à tecnologia autorizada):

```csharp
[JsonPropertyName("res-type")]
public string ResourceType { get; init; } = "data-record";

[JsonPropertyName("resource-name")]
public string? ResourceName { get; init; }

[JsonPropertyName("content-type")]
public string ContentType { get; init; } = "";
```

`id` e `channel_id` da SQLite **nunca** são propriedades dos DTOs públicos. A API usa `resource-name` dentro do escopo do caminho.

## 5. Notificação (contrato interno proposto)

```json
{
  "event": "creation",
  "resource-path": "/api/solid/app1/channel1/dr1",
  "data-record": {
    "res-type": "data-record",
    "resource-name": "dr1",
    "content-type": "application/json",
    "content": "{\"humidity\":45}",
    "dt-creation": "2026-10-27T09:00:00Z",
    "dt-expiration": null
  }
}
```

O enunciado determina **três informações** na notificação: tipo de evento, recurso afetado, caminho completo. As chaves `event`, `resource-path`, `data-record` do envelope são **propostas**, não nomes obrigatórios pelo professor. Manter igual payload nos transportes HTTP/MQTT facilita a Valve e os testes, mas exige acordo da equipa.

## 6. Validações críticas

- `res-type`: obrigatório, válido e compatível com a profundidade/pai.
- `resource-name`: se vazio/duplicado, gerar nome único; reservar `subs` nos records enquanto a regra de routing não for clarificada (Q-21).
- `evt`: inteiro `1` (creation) ou `2` (deletion), até esclarecer contradição em Q-01.
- `endpoint`: URL HTTP recebendo POST **ou** identificador de broker MQTT (como distinguir: Q-08).
- Datas ISO 8601, incluindo `dt-creation` gerado pelo servidor.
- `content`: string; `content-type` indica formato interno dessa string.
- Nunca emitir campos SQL internos no JSON.
