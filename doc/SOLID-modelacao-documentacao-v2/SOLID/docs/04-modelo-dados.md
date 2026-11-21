# 04 — Modelo de dados SQLite (v2)

**[PROPOSTA]** Quatro tabelas porque existem quatro tipos de recursos persistentes. Nomes SQL são internos; contratos públicos usam nomes JSON **exatos**. Ver `18-contratos-dto-e-mapeamento.md` e diagramas `01`/`02`.

| Tabela | Atributos internos | Propriedades REST expostas |
|---|---|---|
| `applications` | `id` PK, `resource_name` único, `resource_desc`, `created_at` | `res-type`, `resource-name`, `resource-desc`, `dt-creation` |
| `channels` | `id` PK, `application_id` FK, `resource_name`, `resource_desc`, `created_at` | os mesmos do tipo `application` |
| `data_records` | `id` PK, `channel_id` FK, `resource_name`, `content_type`, `content`, `created_at`, `expires_at` | `res-type`, `resource-name`, `content-type`, `content`, `dt-creation`, `dt-expiration` |
| `subscriptions` | `id` PK, `channel_id` FK, `resource_name`, `evt`, `endpoint`, `created_at`, `expires_at` | `res-type`, `resource-name`, `evt`, `endpoint`, `dt-creation`, `dt-expiration` |

## Relações e chaves

- `Application (1) → (0..*) Channel` para aceitar o estado temporário após criar a aplicação; o enunciado descreve 1..N canais como relação desejada (Q-18).
- `Channel (1) → (0..*) DataRecord` e `(0..*) Subscription`.
- `id`, `application_id`, `channel_id` são privados. URLs e respostas públicas **nunca** expõem PK/FK.
- `res-type` pode ser **derivado da tabela** na serialização da resposta, sem coluna SQL adicional. Isto resulta do modelo de tabelas por tipo, **não** do facto de haver um middleware único.
- `subs` é nó **virtual** de caminho, não tabela adicional.

## Unicidade e validações no protótipo v2

1. `applications.resource_name` globalmente único na raiz.
2. `(application_id, resource_name)` único para canais.
3. `(channel_id, resource_name)` único em records e em subs, **e também entre ambos** por triggers cruzados [PROPOSTA — Q-03].
4. Reservar `subs` como `resource-name` de record [PROPOSTA — Q-21].
5. `subscription.evt IN (1,2)` **[REQUISITO]** §2.4.
6. Índices de expiração e subscrições para consulta.
7. `ON DELETE CASCADE` **[PROPOSTA — Q-04]**; cascata SQL sozinha **não** envia notificações.

O middleware deve gerar um nome diferente no **POST** quando vazio/duplicado ou reservado, antes de inserir. Os triggers são apenas barreira de integridade; um erro SQL não substitui a resposta JSON adequada.

## Datas e conteúdo

`created_at` é gerado **pelo servidor** e devolvido como `dt-creation` ISO 8601. `expires_at` aceita ausência e representa `dt-expiration` opcional dos dois tipos que expiram. [PROPOSTA] Normalizar internamente para UTC antes de filtrar; comparações lexicais são incorretas se offsets variados forem misturados. `content` armazena *texto* mesmo quando MIME é `application/json`/`application/xml`.

## Estado e ficheiros

`sql/schema.sql` e `sql/seed.sql`: protótipo recriável. `data/solid-modelo-exemplo.db`: demonstração com `app1` → `channel1` → `dr1` e `sub1`, **não** base operada por uma API .NET. Regras e exceções em `19-politica-nomes-e-cascatas.md`.
