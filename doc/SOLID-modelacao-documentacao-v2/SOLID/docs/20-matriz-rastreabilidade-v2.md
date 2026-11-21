# 20 — Rastreabilidade de requisitos, diagramas e testes

| Requisitos | Fundamentação / modelo | Diagramas principais | Evidência a produzir na implementação |
|---|---|---|---|
| SOL-F01..F04, SOL-F16 | 04, 05, 18 | 01, 02, 05, 11 | POST de cada tipo; dispatch por `res-type`; nomes oficiais |
| SOL-F05..F08, SOL-F14 | 05, 19 | 01, 10, 13 | GET/PUT/DELETE e nomes duplicados/gerados |
| SOL-F09..F12 | 06 | 04, 06, 10 | Discovery recursivo, `[]`, `/subs`, sem Get all |
| SOL-F13, SOL-F15 | 07, 19 | 02, 09, 13 | ISO, remoção expirada, sem polling da Valve |
| SOL-F17..F20 | 07, 18 | 05, 07, 12, 13 | JSON de notificação, HTTP POST, MQTT tópico canal |
| SOL-F21..F23 | 05, 14, 18 | 03, 11 | Respostas completas e Swagger/OpenAPI, sem ID SQL |
| SOL-F24..F30 | 08, 16 | 03, 07, 08 | Sensor 1/s 0..100, Valve por notificação, XML e XSD |

**Estado:** esta matriz descreve *o que testar*, não relata testes realizados. Para fechar dúvidas sobre comportamento não fixado, consultar `13-duvidas-e-decisoes.md`.
