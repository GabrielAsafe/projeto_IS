# 06 — Descoberta e endereçamento

## Regras obrigatórias

- O cabeçalho `solid-discovery` contém um de: `application`, `channel`, `data-record`, `subscription`.
- `GET` normal (sem cabeçalho) devolve o **recurso alvo**.
- `GET` com cabeçalho devolve **array JSON de caminhos absolutos relativos à API** que correspondem ao tipo pretendido *dentro do escopo* do URI alvo.
- O escopo pode ser `/api/solid`, uma aplicação, um canal ou outro recurso.
- Procura recursiva pelos descendentes existentes; jamais uma rota genérica `Get all`.
- Quando o alvo não pode conter o tipo pedido, devolver `200 OK` com `[]`.
- Cada subscription descoberta tem caminho `/api/solid/{app}/{channel}/subs/{name}`.
- Subscrições não devem ser tratadas como data-records no encaminhamento de `/subs`.

## Matriz de escopo (resultado típico)

| Alvo | discovery=application | discovery=channel | discovery=data-record | discovery=subscription |
|---|---|---|---|---|
| Raiz `/api/solid` | apps descendentes | canais descendentes | dados descendentes | subs descendentes |
| `/api/solid/{app}` | `[]` | canais | records | subs |
| `/api/solid/{app}/{ch}` | `[]` | `[]` | records | subs |
| Caminho de `data-record` | `[]` | `[]` | `[]` | `[]` |
| Caminho de `subscription` | `[]` | `[]` | `[]` | `[]` |

**[QUESTÃO]** A matriz assume que a descoberta incide sobre *descendentes*, não incluindo o próprio recurso alvo; confirmar este ponto ao testar os exemplos oficiais.

## Algoritmo conceptual

1. Resolver caminho para alvo; validar árvore e tratamento de `subs`.
2. Se `solid-discovery` estiver presente, validar valor.
3. Descobrir descendentes do tipo pretendido no subárvore do alvo (uma consulta à BD por escopo ou travessia controlada).
4. Projetar cada resultado em caminho público usando **nomes**, não IDs.
5. Devolver lista de strings JSON; opcionalmente ordenar para tornar testes determinísticos.
