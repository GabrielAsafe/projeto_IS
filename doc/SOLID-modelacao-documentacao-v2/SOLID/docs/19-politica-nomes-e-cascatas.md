# 19 — Políticas de nomes, eliminação, eventos e expiração (v2)

**Nota:** as políticas marcadas [PROPOSTA] são escolhas **provisórias** destinadas a tornar o protótipo SQL coerente. Não são confirmação do professor.

## 1. Unicidade: o que se sabe

**[REQUISITO]** `resource-name` é único dentro do **pai**, não global. A API identifica recursos por nomes/caminhos; quando o nome está vazio ou é duplicado, o middleware **gera um nome único** e devolve-o. Um canal chamado `channel1` pode existir em duas aplicações distintas.

**[PROPOSTA V2 — interpretação estrita Q-03]** No mesmo canal, o conjunto de nomes de `data-records` e `subscriptions` não se pode sobrepor. O esquema implementa `UNIQUE(channel_id, resource_name)` em cada tabela **e triggers intertabelas** para bloquear colisões cruzadas. Apesar de `/subs` permitir caminhos públicos diferentes, esta regra conservadora segue a leitura literal «dentro do pai». Se o professor permitir reutilização entre tipos, remover os triggers cruzados.

**[PROPOSTA V2 — Q-21]** `data-record.resource-name` não pode ser `subs` (sem distinguir maiúsculas), porque a rota direta `/.../{channel}/subs` está reservada como nó virtual. Quando alguém pedir esse nome, o serviço deve gerar um nome alternativo válido, sem devolver erro por duplicação. Esta política não está definida explicitamente no enunciado.

**[PROPOSTA]** gerar nome antes de persistir e, perante corrida/violação de unicidade, tentar outro; os triggers são última defesa e não substituem o tratamento do contrato.

## 2. Cascata — Q-04

A v2 mantém `ON DELETE CASCADE` em `channels`, `data_records` e `subscriptions` para assegurar integridade referencial ao apagar pai. **Isto é infraestrutura, não semântica pública completa**. É preciso confirmar:

- se o DELETE da application/channel deve ser permitido quando existem filhos;
- se a remoção de filhos por cascata emite `evt=2` para cada data-record apagado;
- que subscrições são destinatárias antes de serem também removidas.

## 3. Evento de deletion — Q-05

A descrição geral diz que a eliminação de um data-record desencadeia notificação. Para DELETE direto, a arquitetura deverá construir um **snapshot do record antes da remoção**, efetuar COMMIT e publicar com `event=deletion` e path original.

**Ainda por confirmar:** remoção automática por TTL e remoção indireta por cascata devem publicar eventos? A v2 mantém ambos como decisões, **sem afirmar cobertura funcional implementada**. SQL com FK/trigger de integridade não envia HTTP/MQTT por si só.

## 4. Expiração — TTL

**[REQUISITO]** somente `data-record` e `subscription` possuem `dt-expiration`; ausente/vazio = sem expiração. Com data válida, middleware remove quando hora atual ultrapassa vencimento. **[PROPOSTA]** armazenar em ISO 8601 UTC uniforme e usar `BackgroundService` com timer periódico. O timer interno não é o polling de medição proibido na Valve.

A comparação segura de datas requer normalizar offsets/timezones em vez de comparar livremente strings ISO 8601 com representações heterogéneas. A coluna SQL é `TEXT`, mas o serviço deve interpretar temporalmente o valor e guardar representação normalizada (idealmente UTC).

## 5. Consequências para testes

| Teste | Esperado pelo requisito / decisão |
|---|---|
| Nomes iguais em aplicações distintas | permitido (ex.: `channel1` de `app1` e `app2`) |
| Nome duplicado no mesmo pai | middleware atribui outro nome na **criação** |
| Record e sub com mesmo nome no mesmo canal | impedido **[PROPOSTA Q-03]** |
| Criar record chamado `subs` | gerar alternativa **[PROPOSTA Q-21]** |
| Excluir record diretamente | emitir evento deletion `evt=2` para subscrições elegíveis |
| Eliminar canal com records | integridade via cascata **[PROPOSTA Q-04]**; eventos a decidir |
| Expirar subscription | recurso removido; não é evento de record |
| Expirar data-record | recurso removido; política de notification **[QUESTÃO Q-05]** |
