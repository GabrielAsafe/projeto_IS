# 08 — Cenário obrigatório: rega automática

## Atores e aplicações

- **Moisture Sensor Application**: aplicação independente; simula uma medição de humidade `0 ≤ h ≤ 100` de segundo a segundo, publicando cada medição como data-record num canal SOLID.
- **Water Valve Application**: aplicação independente; configura limiar `T` (unidades percentuais), regista subscrição do tipo `evt=1`, recebe notificações do middleware e decide estado da válvula.
- **SOLID Middleware**: único intermediário para qualquer comunicação entre as duas simulações.

## Regra de decisão

`humidity < threshold` ⇒ **ON**; `humidity >= threshold` ⇒ **OFF**. Limiar igual à medição deve desligar a válvula. Os valores 0 e 100 são válidos.

## Fluxo principal

1. Criar aplicação + canal de medições no middleware.
2. Iniciar Water Valve Application, configurar limiar e criar `subscription` de `creation` no canal.
3. Iniciar Moisture Sensor Application, produzir 1 valor por segundo e criar `data-record` com a medição (nomes únicos auto-gerados ou atribuídos).
4. SOLID persiste record e emite notificação HTTP ou MQTT.
5. Válvula recebe notificação, extrai `content` do data-record (ou consulta recurso identificado no evento), valida medição, atualiza ON/OFF.
6. **Todas** as notificações recebidas são persistidas no ficheiro XML, validado contra XSD definido pela equipa.

## Pontos de atenção

- Válvula **não** pode consultar continuamente o canal à procura de novas medições, nem fazer busy-wait, polling ou scanning periódico.
- O temporizador do **sensor** de 1 segundo é permitido e obrigatório.
- É permitido obter o conteúdo do data-record através da URI **quando se recebe uma notificação**. É um GET desencadeado por evento, não polling.
- Guardar notificações inválidas no XML? O enunciado diz **todas recebidas** e inválidas perante XSD; definir se são guardadas em área separada ou rejeitadas com log (Q-09).

## XML e XSD

O enunciado **não fornece** esquema XSD; deve ser criado pela equipa. Proposta de raiz `<notifications>` com elementos repetidos `<notification>`, `<event>`, `<resourcePath>`, `<receivedAt>`, `<humidity>`, mas a estrutura e o tratamento de inválidos devem ser acordados antes de implementar.

## Testes especialmente importantes

Valores limite 0%, 100%, `h=T`, abaixo/acima; frequência de publicação; perda temporária do destino; payload inválido; XML/XSD válido; comprovação que não há polling.
