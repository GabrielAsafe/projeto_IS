# 12 — Plano técnico e entrega final

## Critérios de avaliação (transcrição estruturada de §5)

A tabela do enunciado apresenta: **Middleware 60%** (4 tipos de recursos com CRUD/Discovery, 15% cada); **remoção por `dt-expiration` 5%**; **Testing application 20%**; **Report 15%**. O quadro tem uma ambiguidade visual na hierarquia do item de expiração; em conjunto, os itens enumerados somam 100% quando se tratam os 5% separadamente. Confirmar leitura com o docente.

## Etapas sugeridas (não há sprints definidos no enunciado)

1. **Modelação/contrato:** hierarquia, nomes públicos exatos, DTOs, caminhos, ambiguidades, SQLite, Swagger básico.
2. **CRUD:** application e channel, depois data-record e subscription; testes de nomes/IDs.
3. **Discovery e expiração:** header, paths recursivos, serviço de limpeza, casos de erro.
4. **Notificações:** criação/remoção, callbacks HTTP POST, publicação MQTT, transporte testado.
5. **Cenário obrigatório:** sensor e válvula independentes; XML, XSD, lógica ON/OFF e E2E sem polling.
6. **Hardening:** dados obrigatórios, SQL de recriação, documentação Swagger, comandos cURL exaustivos, relatório e discussão individual.

## Entrega (§7)

Submeter na página da UC um **link para um único ZIP ou 7z externo** com:

```text
identification.txt
Project/    # código fonte completo, dependências e recursos necessários
Report/     # relatório conforme template oficial, PDF ou DOCX
Other/      # XML/XSD e recursos relevantes não colocados acima
Data/       # ficheiro SQLite .db + scripts .sql para recriar e popular
```

O relatório inclui apêndice obrigatório a identificar **quais as funcionalidades implementadas por cada elemento da equipa**. O template e a data exata constam da página/agenda oficial e **não foram fornecidos** nesta mensagem.

## Checklist de apresentação/discussão individual

- Explicar endpoints e diferença entre GET normal e discovery.
- Mostrar porque `/subs` é virtual e os IDs internos não aparecem.
- Demonstrar eventos HTTP/MQTT e resolução do tópico.
- Demonstrar a válvula a reagir só a eventos, sem polling.
- Validar XML perante XSD.
- Abrir DB com os quatro recursos obrigatórios, demonstrar expiração e reconstrução SQL.
- Executar todos os exemplos cURL e navegar Swagger.
