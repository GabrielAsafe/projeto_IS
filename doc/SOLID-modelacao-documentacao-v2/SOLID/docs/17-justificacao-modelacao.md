# 17 — Fundamentação da modelação e da arquitetura (v2)

## 1. Fontes e grau de certeza

- **[REQUISITO]** O enunciado §2 especifica quatro tipos de recurso, hierarquia máxima de três níveis, nomes JSON exatos, REST/JSON e discovery por header.
- **[REQUISITO]** O enunciado §§2 e 2.4 especifica .NET 10, SQLite, porta 8765, timestamp e expiração, notificações HTTP/MQTT e Swagger/OpenAPI.
- **[REQUISITO]** O enunciado §4 exige apps independentes Sensor e Valve a comunicar **só pelo middleware**, medições a cada segundo, regra do limiar e XML/XSD.
- **[PROPOSTA]** Monólito modular com separação API / Aplicação / Domínio / Infraestrutura; não é pedido usar microserviços nem Clean Architecture integral.

## 2. Porquê monólito modular em vez de microserviços?

O contrato é único (`/api/solid`), a persistência exigida é SQLite e existem apenas quatro tipos persistidos, fortemente relacionados. Separar serviços em processos complicaria consistência, implantação, discovery e tratamento de notificações sem benefício imposto pelo enunciado. **Modular** não significa que todas as responsabilidades sejam misturadas: os módulos são classes/componentes do mesmo processo API. **Sensor** e **Valve** são aplicações independentes (outros processos), como exige §4.

## 3. Porque estes quatro recursos e quatro tabelas?

A árvore do enunciado dá os agregados e suas relações: Application → Channel → (DataRecord, Subscription). Quatro tabelas simplificam restrições específicas, chaves estrangeiras e eliminação; uma única tabela genérica seria possível mas teria muitos atributos nulos e validações condicionais. `subs` é apenas nó virtual de URI; **não** cria a quinta entidade.

## 4. Porque DTOs, e porquê `res-type`?

A API de criação endereça **o pai**. Sob o mesmo canal, `POST /api/solid/{app}/{channel}` pode criar tanto `data-record` como `subscription`; o campo JSON **obrigatório** `res-type` discrimina o pedido. O servidor faz parsing do envelope e valida um **DTO concreto**, p.ex. `CreateDataRecordDto` ou `CreateSubscriptionDto`. Este é um padrão possível de implementação; o facto de ter um middleware único **não obriga**, por si, ao discriminador. O discriminador decorre do **contrato público** do enunciado. DTOs isolam JSON com chaves hifenizadas de entidades SQL com `id` e FKs privados.

## 5. Porque serviços separados?

- `ResourceApplicationService`: criação, validação de nome, invariantes, atualização/remoção.
- `DiscoveryService`: consulta recursiva e projeção de *paths*, sem modificar dados.
- `NotificationCoordinator`: filtro de `evt`, dados do evento e escolha HTTP/MQTT após persistência.
- `ExpirationWorker`: manutenção interna de TTL; **não** é polling de medições pela Valve.
- `Repositories`: contrato de acesso a SQLite, que não é objeto público.

São nomes conceptuais; a solução pode usar menos projetos/classes, conforme as aulas.

## 6. Separação dos diagramas

- `01-diagrama-classes`: **domínio**, sem serviços.
- `02-diagrama-er`: tabelas/PK/FK (perspetiva da BD).
- `03-arquitetura-componentes`: processos e módulos.
- `10-diagrama-servicos`: dependências de código.
- `11-diagrama-dtos`: fronteira JSON/domínio e discriminador.
- `05`, `07`, `12`: ordem temporal e contratos de eventos.
- `09`, `13`: processos de limpeza e remoção.

## 7. Riscos que permanecem decisões e não factos

`evt=both` vs `evt` 1/2 (Q-01); nomes entre tipos no mesmo pai (Q-03); cascata (Q-04); notificações de expiração (Q-05); `endpoint` MQTT sem campo de protocolo (Q-08); retry/QoS (Q-13); tecnologias permitidas nas aulas (Q-14). Estão identificados em `13-duvidas-e-decisoes.md` para evitar decisões silenciosas.

## 8. Estado da modelação

Este material é **desenho e protótipo SQL**, não uma implementação operacional da API, do broker nem dos simuladores. O resultado final só pode ser validado por testes reais e feedback docente.
