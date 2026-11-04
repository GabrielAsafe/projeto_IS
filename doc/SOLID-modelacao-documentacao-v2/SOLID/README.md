# SOLID — Documentação e modelação **v2**

**Integração de Sistemas, 2026/2027** — projeto **SOLID: Service-Oriented Layer for Interoperable Data**. Esta pasta é uma **evolução completa da v1** (sem substituir o enunciado). Artefactos de **modelação**, não middleware .NET operacional.

## Entrega da v2

- **13 diagramas Mermaid** em pares `.md` (com descrição) e `.mmd` (editável).
- Diagramas separados de **domínio (`01`)**, **serviços (`10`)**, **DTOs (`11`)** e novos fluxos de **notificação (`12`)** e **eliminação (`13`)**.
- Modelo **SQLite demonstrativo** com esquema/seed e verificações automáticas; inclui `app1/channel1/dr1/sub1`.
- Contratos REST, discovery, JSON e exemplos cURL, análise de stack e entrega preservados da v1.
- Novos ficheiros de **fundamentação arquitetural**, **DTOs**, **políticas de nomes/cascatas**, **rastreabilidade**, **changelog** e **glossário**.

## Por onde começar

1. `docs/17-justificacao-modelacao.md` — porque é assim a arquitetura.
2. `docs/diagramas/01-diagrama-classes.md` e `02-diagrama-er.md` — domínio vs SQLite.
3. `docs/diagramas/03-arquitetura-componentes.md`, `10-diagrama-servicos.md`, `11-diagrama-dtos.md` — responsabilidades e DTOs.
4. `docs/18-contratos-dto-e-mapeamento.md` — explicação de `res-type`.
5. `docs/19-politica-nomes-e-cascatas.md`, `docs/13-duvidas-e-decisoes.md` — decisões **provisórias** e dúvidas do enunciado.
6. `docs/20-matriz-rastreabilidade-v2.md` e `docs/21-changelog-v2.md` — rastreabilidade/mudanças.

## Índice documental

| Grupo | Ficheiros |
|---|---|
| Fonte | `docs/00-enunciado-original.txt` (não alterado) |
| Base | `docs/01` a `docs/16` (requisitos, domínio, persistência, REST, discovery, eventos, cenário, cURL, testes, stack, entrega, OpenAPI, ADRs, XML/XSD) |
| Complementos v2 | `docs/17` fundamentação, `18` DTOs, `19` unicidade e cascatas, `20` rastreabilidade, `21` changelog, `22` glossário |
| UML/fluxos | `docs/diagramas/01` a `13`, em `.md` e `.mmd` |
| SQLite | `sql/schema.sql`, `sql/seed.sql`, `data/solid-modelo-exemplo.db` |
| XML | `exemplos/valve-notifications-example.xml`, `exemplos/valve-notifications.xsd` |
| Verificações | `tests/test_sqlite_model.py` (teste do **modelo SQLite**, não dos serviços) |

## Convenções de certeza

- **[REQUISITO]**: vem do enunciado fornecido.
- **[PROPOSTA]**: é uma escolha de modelação **reversível**.
- **[QUESTÃO]**: lacuna ou inconsistência, precisa de esclarecimento.
- `SOL-Fxx`: identificador **interno de rastreabilidade**, não atribuído pelo docente.

## Limites fixos de conformidade

.NET 10, SQLite, URL `/api/solid`, porta 8765, nomes públicos JSON exatos, descoberta com `solid-discovery`, nó virtual `/subs`, sem `GET all`, REST/JSON, eventos HTTP/MQTT, Swagger/OpenAPI, cenário Sensor/Valve e XML/XSD. §6: apenas tecnologias ensinadas nas aulas práticas. Questões Q-01/Q-02/Q-03/Q-05/Q-08/Q-14 **não** são decisões aprovadas.

**Importante:** a base `.db` é apenas um artefacto de prototipagem. O pacote não inclui implementação funcional do middleware/Valve/Sensor nem evidência de validação docente.
