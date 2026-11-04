# 07 — Notificações HTTP/MQTT, contrato e expiração (v2)

## Obrigatório pelo enunciado

- Criar um `data-record` gera evento de **creation**; eliminar gera **deletion**.
- `subscription.evt = 1` significa creation e `2` significa deletion (§2.4). A menção anterior a "ambos" permanece contraditória (Q-01).
- As notificações devem conter **tipo de evento, recurso afetado e caminho completo**.
- HTTP: `endpoint` é URL que recebe `POST`.
- MQTT: `endpoint` armazena endereço do **broker**. O **tópico** é calculado *em runtime* a partir do caminho do canal, p.ex. `api/solid/app1/channel1`.
- `data-record` e `subscription` podem ter `dt-expiration`; recurso vencido deve ser automaticamente removido.

## Contrato de evento proposto (o mesmo conteúdo em HTTP e MQTT)

```json
{
  "event": "creation",
  "resource-path": "/api/solid/app1/channel1/dr1",
  "data-record": {
    "res-type": "data-record",
    "resource-name": "dr1",
    "content-type": "application/json",
    "content": "{\"humidity\":45}",
    "dt-creation": "2026-10-27T09:00:00Z",
    "dt-expiration": null
  }
}
```

As **chaves do envelope da notificação** são proposta da equipa, ao contrário das propriedades **internas do recurso**, cujos nomes são impostos.

## Encaminhamento e separação de responsabilidades

1. REST valida DTO, `res-type` e pai; ResourceApplicationService trata persistência.
2. Guardar/comitar record em SQLite.
3. Notificar `NotificationCoordinator` **depois** da persistência; identificar subscrições do canal filtradas por `evt`.
4. Adaptador HTTP envia JSON por `POST`; adaptador MQTT publica JSON no tópico calculado para o broker que figura em `endpoint`.
5. Receiver externo (ex.: Valve) consome *push* (sem polling); pode consultar record por GET **após** a notificação, se necessário.

**Nota:** enviar após commit sem outbox significa que uma falha da aplicação entre commit e envio pode perder eventos. Exatamente qual a garantia de entrega, retry e QoS está em aberto (Q-13). Não inventamos semântica de entrega confirmada.

## Como distinguir HTTP de MQTT? (Q-08)

Sem um campo público `protocol`, só podemos propor convenção sobre o valor do `endpoint` ou outra regra interna de despacho. Por exemplo, `http://`/`https://` → HTTP e um URI válido de broker `mqtt://...` → MQTT **[PROPOSTA SUJEITA A APROVAÇÃO]**. Não guardar o tópico na propriedade `endpoint` MQTT. Confirmar na disciplina se o broker/cliente ensinado aceita os esquemas propostos.

## Worker de expiração

[PROPOSTA] serviço interno `BackgroundService` + `PeriodicTimer` consulta a SQLite, remove records/subs com expiração anterior ao momento atual e respeita uma política de eliminação/avisos. Isto **não é** o polling proibido na Valve, pois a Valve deteta medições somente por notificações. `BackgroundService` implementa `IHostedService`; não são tecnologias concorrentes de igual nível.

## Eliminações: não assumir uma política que não existe

- `DELETE` explícito de data-record: emitir deletion (com snapshot pré-eliminação para não perder o conteúdo).
- Expiração de data-record: se também deve emitir deletion, **Q-05**.
- Cascade delete de application/channel: comportamento da base [PROPOSTA]; eventuais mensagens de records filhos **Q-04/Q-05**.
- Eliminar subscription: não gera evento de data-record.

Referências: `18-contratos-dto-e-mapeamento.md`, `19-politica-nomes-e-cascatas.md` e diagramas `05`, `09`, `12`, `13`.
