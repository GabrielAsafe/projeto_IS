# 13 — Ambiguidades, decisões e hipóteses (v2)

**Estado inicial:** todas as propostas devem ser validadas com o docente ou através de critérios oficiais. Não alterar nomes públicos para «corrigir» o enunciado sem validação.

| ID | Questão | Enunciado / motivo | Abordagem de trabalho (provisória) |
|---|---|---|---|
| Q-01 | `evt` pode representar «ambos»? | §2.1 diz creation/deletion/both; §2.4 determina apenas 1 ou 2 | Implementar apenas 1 e 2; obter esclarecimento |
| Q-02 | Campo de data de subscription é `dtcreation`? | Uma tabela §2 escreve `dtcreation`; §2.4 determina `dt-creation` para todos | Usar `dt-creation` consistentemente |
| Q-03 | Nome único atravessa os dois tipos de filho do canal? | Escopo do recurso pai; URIs distintos graças a `/subs` | **V2:** triggers cruzados impõem nomes diferentes entre record/sub no mesmo canal [PROPOSTA]; perguntar |
| Q-04 | Ao apagar channel/application, apagar descendentes? | A hierarquia não fixa o comportamento de cascata | **V2:** SQL com `ON DELETE CASCADE` [PROPOSTA]; política de eventos pendente |
| Q-05 | Deleção automática por expiração gera notificação? | Eventos de deletion de data-record; expiração remove recurso | Escalonar decisão; evitar comportamento silencioso não documentado |
| Q-06 | Valor de `dt-expiration` quando vazio é `null` ou `""`? | §2.4 permite omitir/deixar vazio | Aceitar `null`/ausente, converter conforme contrato acordado |
| Q-07 | Qual o formato exato do payload de notificação? | Só 3 componentes obrigatórios descritos | Definir esquema JSON, documentar e integrar ambos simuladores |
| Q-08 | Como distinguir endpoint HTTP vs MQTT? | HTTP URL vs MQTT broker endpoint, sem campo protocol | Propor esquema URI; validar com professor |
| Q-09 | O que fazer com notificações inválidas no XML? | Persistir todas as recebidas; schema invalida as não conformes | Guardar raw separadamente ou tratar erros; validar interpretação |
| Q-10 | Recolha da descoberta inclui o recurso-alvo? | Fala de «within scope» e recursividade | Interpretar como descendentes, sem auto-inclusão |
| Q-11 | É possível atualizar nome do recurso por PUT? | Recursos app/channel atualizáveis, identificação por nome | Priorizar atualizar descrição; perguntar sobre renomeação |
| Q-12 | Qual o status de DELETE e erros? | Pede status adequado, não codifica todos | Convencionar 201/200/204/400/404/405, documentar |
| Q-13 | Retentativas, QoS, ordenação e garantias de entrega? | Não especificados | Versão simples suficiente e medição de falhas |
| Q-14 | Tecnologias de bibliotecas autorizadas? | §6 limita a tecnologias das aulas práticas | Confirmar EF Core, MQTTnet, Swagger UI, testes, IDE |
| Q-15 | Cumprimento da notificação MQTT: broker externo ou embutido? | `endpoint` deve armazenar broker | Preferir broker ensinado em aula, sem fornecedor obrigatório |
| Q-16 | Há endpoint GET da raiz sem header? | `Get all` expressamente proibido | Não devolver coleção; erro/documentação explícitos |
| Q-17 | A demo precisa de ambos os transportes com a válvula? | Middleware suporta HTTP + MQTT; caso exige notifications | Implementar ambos no middleware; demonstrar no cenário pelo menos um, confirmar |
| Q-18 | Limite de canais por application | «one or more» mas remover último canal poderia contrariar cardinalidade | Exigir ao criar? validar com docente; BD só garante FK |
| Q-19 | HTTP «application/json» em JSON que é `content` textual | `content-type` de data-record é MIME do conteúdo, corpo REST é JSON | Guardar texto e serializar uma string em JSON, mesmo para XML/JSON |
| Q-20 | Validade da apresentação formal de data/hora ISO | Exemplo sem timezone; §2.4 exige ISO 8601 | Usar UTC ISO 8601 e documentar representação/exibição |
| Q-21 | Pode haver um data-record chamado `subs`? | A rota direta de records colide com o nó virtual de subscriptions | **V2:** proibir `subs` como nome de record por CHECK e gerar nome alternativo [PROPOSTA] |

## Registo de revisões v2

- **Q-03 e Q-21:** agora existe uma regra provisória **implementada no protótipo SQL**, não aprovada pelo docente. Consultar `19-politica-nomes-e-cascatas.md`.
- **Q-04/Q-05:** integridade referencial e notificações são assuntos distintos; a cascata não implica callback.
- **Q-07/Q-08/Q-13:** contrato JSON é proposto, URI broker/garantias de entrega pendentes.
- **Q-18:** diagramas de domínio representam 0..* canais durante a fase inicial de configuração.

## Ordem recomendada para resolução

Prioridade alta: Q-01, Q-02, Q-03, Q-05, Q-08 e Q-14. Prioridade média: Q-04, Q-06, Q-07, Q-10, Q-11. As restantes podem ser esclarecidas durante spikes e testes.
