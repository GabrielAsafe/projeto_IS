# 02 — Requisitos funcionais rastreáveis

Identificadores próprios da documentação, não atribuídos pelo docente. Fonte: enunciado, secções 2–4.

| ID | Requisito | Fonte |
|---|---|---|
| SOL-F01 | Registar `application` ao nível da raiz | §2 / §2.1 |
| SOL-F02 | Criar `channel` dentro de `application` | §2 / §2.1 |
| SOL-F03 | Criar `data-record` num `channel` | §2 / §2.1 |
| SOL-F04 | Criar `subscription` num `channel` | §2 / §2.1 |
| SOL-F05 | Consultar recurso pelo `resource-name` e caminho | §2.1 |
| SOL-F06 | Atualizar `application` e `channel` | §2.1 |
| SOL-F07 | Eliminar recursos pelo caminho do alvo | §2.1 |
| SOL-F08 | Rejeitar update de `data-record` e `subscription`; recriar para alterar | §2.1 |
| SOL-F09 | Descobrir recursos recursivamente com `GET` + `solid-discovery` | §2.2–2.3 |
| SOL-F10 | Na descoberta de subscrições incluir `/subs/` em todos os caminhos | §2.1–2.2 |
| SOL-F11 | Responder `200` e `[]` para descoberta logicamente impossível | §2.3 |
| SOL-F12 | Não disponibilizar endpoint genérico de listagem | §2.3 |
| SOL-F13 | Atribuir `dt-creation` automaticamente no formato ISO | §2.4 |
| SOL-F14 | Gerar `resource-name` se omitido ou duplicado no mesmo âmbito | §2.4 |
| SOL-F15 | Expirar/remover automaticamente data-records e subscriptions com `dt-expiration` | §2.4 |
| SOL-F16 | Aceitar exclusivamente `res-type` válido no corpo e preservar nomes oficiais | §2 / §2.4 |
| SOL-F17 | Publicar evento da criação de data-record a subscritores correspondentes | §2 |
| SOL-F18 | Publicar evento da eliminação de data-record a subscritores correspondentes | §2 |
| SOL-F19 | Enviar notificações via callback HTTP POST ou MQTT broker | §2.1 / §2.4 |
| SOL-F20 | Incluir evento, recurso afetado e caminho completo na notificação | §2.1 |
| SOL-F21 | Devolver recurso completo após criação/atualização, incluindo nome gerado | §2.1 |
| SOL-F22 | Persistir recursos e dados em SQLite, sem expor IDs internos | §2.1 / §2.4 |
| SOL-F23 | Expor Swagger/OpenAPI completo e correto | §2.4 |
| SOL-F24 | Simular humidade entre 0 e 100 a cada segundo | §4 |
| SOL-F25 | Publicar cada amostra como data-record no middleware | §4 |
| SOL-F26 | Configurar o limiar de humidade da válvula | §4 |
| SOL-F27 | Subscrever eventos de criação e ligar/desligar válvula conforme limiar | §4 |
| SOL-F28 | Guardar todas as notificações recebidas pela válvula num XML | §4 |
| SOL-F29 | Definir e validar o XML perante XSD da equipa | §4 |
| SOL-F30 | Aplicações de simulação comunicam exclusivamente pelo middleware | §4 |

## Regras de aceitação transversais

- Para todas as quatro categorias, criação e descoberta usam URI do pai; consulta e remoção usam URI do alvo.
- Apenas `application` e `channel` são atualizáveis.
- Descoberta percorre descendentes do âmbito especificado (sem `Get all`).
- O endpoint de subscrição é armazenado; o tópico MQTT é derivado do caminho do canal em tempo de execução.
- Nenhuma resposta deve expor chaves primárias SQLite.

## Estratégia de verificação

Mapear estes IDs para testes na matriz `10-testes-casos.md`; exemplos concretos em `09-exemplos-curl.md`. A secção `13-duvidas-e-decisoes.md` identifica requisitos com contradições de detalhe.
