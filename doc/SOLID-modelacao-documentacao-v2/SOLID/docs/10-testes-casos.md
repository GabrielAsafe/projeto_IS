# 10 — Matriz inicial de testes

**[PROPOSTA]** Casos de teste derivados do enunciado. A UC não impõe BDD/Gherkin/Katalon neste projeto; usar tecnologias de teste efetivamente lecionadas.

| ID | Cenário | Verificação essencial | Requisitos |
|---|---|---|---|
| T-01 | Criar application | 201, nome explícito, dt-creation, DTO sem PK | F01, F13, F22 |
| T-02 | Nome omitido | gera e devolve nome | F14, F21 |
| T-03 | Nome duplicado no pai | gera nome único e devolve | F14 |
| T-04 | Criar channel sem application existente | falha com erro adequado | F02 |
| T-05 | Atualizar application/channel | representação atualizada | F06, F21 |
| T-06 | Atualizar data-record/subscription | rejeição; recurso inalterado | F08 |
| T-07 | Ler record pelo URL | apenas campos públicos | F05, F22 |
| T-08 | Ler sub via `/subs/` | sucesso; outro URL não é sub | F05, F10 |
| T-09 | Descoberta recursiva a partir da raiz | caminhos corretos por tipo | F09 |
| T-10 | Descoberta sub | inclui `/subs/` | F10 |
| T-11 | Discovery impossivel sobre record | 200 e `[]` | F11 |
| T-12 | GET sem header | devolve único recurso, não array de descendentes | F12 |
| T-13 | Criar record gera callback creation | `evt=1`, URI e record completos | F17, F19, F20 |
| T-14 | Eliminar record gera callback deletion | `evt=2`, URI e record completos | F18, F19, F20 |
| T-15 | Evento não subscrito | não enviar à sub de outro evento | F17, F18 |
| T-16 | MQTT | publica no broker endpoint e tópico do canal | F19 |
| T-17 | Expiração record | removido automaticamente depois do prazo | F15 |
| T-18 | Expiração subscription | deixa de receber notificações após prazo | F15 |
| T-19 | Sem expiração | não remover automaticamente | F15 |
| T-20 | Sensor emite 1 leitura/segundo | valores 0..100, data-records | F24, F25 |
| T-21 | Medição abaixo do limiar | válvula ON por notificação | F26, F27 |
| T-22 | Medição igual ao limiar | válvula OFF | F26, F27 |
| T-23 | Medição superior ao limiar | válvula OFF | F26, F27 |
| T-24 | XML válido | todas as notificações guardadas e XSD validado | F28, F29 |
| T-25 | Fluxo sem polling | ver logs/code paths: só eventos disparam decisão | F30 |
| T-26 | Sample DB | `app1`, `channel1`, `dr1`, `sub1` presentes | §2.4 |
| T-27 | Swagger | todos os recursos, headers, statuses, schemas | F23 |

## Estratégia de teste recomendada

1. **Unidade:** nomes, regras, expiração, cálculo ON/OFF e serialização DTO.
2. **Integração (SQLite real):** FKs, transações, criação/descoberta/remoção, timestamps.
3. **Contrato HTTP:** cURL/Postman e documentação OpenAPI; diferenciação com e sem header.
4. **E2E:** middleware + sensor + válvula + transporte + XML/XSD.

## Critério de pronto sugerido

Teste automatizado verde, Swagger coerente, exemplos cURL executados, dados SQLite corretos e relatório preparado com evidências reais (logs ou capturas). O enunciado não define uma Definition of Done formal.

## V2 — Verificações adicionais a automatizar

- JSON com `res-type` inválido ou incompatível com o pai ⇒ não criar recurso.
- POST ao canal com ambos os tipos ⇒ DTO especializado correto; `resource-name` gerado se duplicado.
- Subscrição/record com o mesmo nome no mesmo canal ⇒ comportamento **[PROPOSTA Q-03]** documentado.
- Record chamado `subs` ⇒ nome alternativo **[PROPOSTA Q-21]**.
- Eliminar record ⇒ payload de deletion conserva dados/URI mesmo após a remoção.
- TTL de data-record ⇒ comportamento de notificação medido após decisão Q-05.
- HTTP e MQTT ⇒ comparabilidade do payload e broker/topic corretos, sem polling.
- Revisão de `13-duvidas-e-decisoes.md` com docente antes de declarar contrato final.

O script `../tests/test_sqlite_model.py` valida somente o **esquema e seed**, não prova rotas REST nem entrega de notificações.
