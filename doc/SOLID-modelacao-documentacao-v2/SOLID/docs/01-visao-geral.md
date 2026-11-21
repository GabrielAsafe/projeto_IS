# 01 — Visão geral e âmbito

**Projeto:** SOLID — Service-Oriented Layer for Interoperable Data  
**Unidade curricular:** Integração de Sistemas, 2026/2027  
**Origem:** secções 1–7 do enunciado disponibilizado.  
**Objetivo:** middleware interoperável que permite publicação, consulta e descoberta de recursos hierárquicos, bem como notificações baseadas em eventos.

## Duas aplicações distintas do middleware

1. **SOLID Middleware:** API REST, persistência SQLite, hierarquia e descoberta de recursos, expiração automática, notificações HTTP/MQTT.
2. **Cenário obrigatório:** simulação independente de `Moisture Sensor Application` (medição de humidade por segundo) e `Water Valve Application` (reação a notificações e registo XML/XSD).

## Restrições explícitas

- **.NET 10** para implementação; tecnologias, frameworks e metodologias apenas das aulas práticas.
- API REST base `http://<host>:8765/api/solid` (prefixo obrigatório `/api/solid`).
- Persistência em **SQLite**, recursos de demonstração obrigatórios `app1/channel1/dr1` e `app1/channel1/subs/sub1`.
- Entradas/saídas REST JSON; `data-record.content` é uma string cujo conteúdo pode ser XML, JSON ou texto consoante `content-type`.
- Swagger/OpenAPI obrigatório.
- Hierarquia limitada a application → channel → (data-record | subscription), com `/subs` como nó virtual para endereço de subscrições.
- **Não** há operação genérica `Get all`; enumeração exclusivamente com `solid-discovery`.
- Simulações comunicam **exclusivamente pelo middleware**; válvula recebe notificações, nunca deteta novas medições por polling.

## Notas de interpretação

As secções com o selo **[REQUISITO]** reproduzem obrigações do enunciado (por paráfrase); **[PROPOSTA]** identifica desenho/arquitetura; **[QUESTÃO]** identifica ambiguidades por validar. Ver `13-duvidas-e-decisoes.md`.

## Fora de âmbito obrigatório (não assumir)

Autenticação, dashboard web, cloud, tecnologia concreta do MQTT broker, API externa de IoT, retries, logs estruturados ou containers não são prescritos no enunciado. Podem ser extensões, sujeitas ao critério das tecnologias lecionadas.
