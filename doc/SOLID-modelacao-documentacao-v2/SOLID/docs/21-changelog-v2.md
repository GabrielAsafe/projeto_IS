# 21 — Histórico da v2 (face à v1)

1. **Separação de diagramas:** `01` passou a representar **apenas domínio**; `10` representa **serviços/dependências**; `11` DTOs/discriminador.
2. **Arquitetura clarificada:** `03` representa monólito modular, Sensor/Valve externos e notificações depois da persistência.
3. **Fluxos atualizados:** `05` criação+DTO; `07` irrigação; `09` expiração; novos `12` notificação HTTP/MQTT e `13` eliminação.
4. **Contrato DTO criado:** `18-contratos-dto-e-mapeamento.md` distingue `res-type`, envelope JSON, DTOs e entidade SQL.
5. **Política de nomes/remoções:** `19-politica-nomes-e-cascatas.md` explica unicidade cruzada [PROPOSTA], reserva de `subs` e cascatas/eventos pendentes.
6. **SQL e BD demonstrativa:** triggers para colisões record/sub e reserva de `subs`, dados amostra reconstruídos e testados localmente (sem API real).
7. **Justificação:** `17-justificacao-modelacao.md` explica escolhas e o que é exigido/proposto/questão.
8. **Rastreabilidade:** `20-matriz-rastreabilidade-v2.md` mapeia funcionalidades a diagramas e evidências futuras.
9. **Siglas e estados:** `22-glossario.md` diferencia o projeto SOLID, ids SOL-F e abreviaturas técnicas.
10. **Compatibilidade:** os documentos e exemplos da v1 foram preservados e atualizados quando necessário; `00-enunciado-original.txt` mantido inalterado.

## O que a v2 **não** é

Não é a implementação funcional .NET 10, nem certificação de compatibilidade com bibliotecas das aulas, nem decisão aprovada pelo professor sobre dúvidas do enunciado. O pacote continua a ser documentação/modelação + protótipo SQLite.
