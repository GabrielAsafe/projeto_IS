# 09 — Exemplos cURL por recurso

**Exemplos de testes para a API que será implementada; não são resultados de execução.** Usam a interface projetada em `05-contrato-rest.md`. Executar após iniciar o servidor na porta 8765.

```bash
BASE='http://localhost:8765/api/solid'

# Application: CREATE / READ / UPDATE / DISCOVER / DELETE
curl -i -X POST "$BASE" -H 'Content-Type: application/json' \
  -d '{"res-type":"application","resource-name":"app1","resource-desc":"IoT"}'
curl -i "$BASE/app1"
curl -i -X PUT "$BASE/app1" -H 'Content-Type: application/json' \
  -d '{"res-type":"application","resource-name":"app1","resource-desc":"IoT updated"}'
curl -i "$BASE" -H 'solid-discovery: application'

# Channel: CREATE / READ / UPDATE / DISCOVER
curl -i -X POST "$BASE/app1" -H 'Content-Type: application/json' \
  -d '{"res-type":"channel","resource-name":"channel1","resource-desc":"Humidity"}'
curl -i "$BASE/app1/channel1"
curl -i -X PUT "$BASE/app1/channel1" -H 'Content-Type: application/json' \
  -d '{"res-type":"channel","resource-name":"channel1","resource-desc":"Updated humidity"}'
curl -i "$BASE/app1" -H 'solid-discovery: channel'

# Data-record: CREATE / READ / DISCOVER (sem UPDATE)
curl -i -X POST "$BASE/app1/channel1" -H 'Content-Type: application/json' \
  -d '{"res-type":"data-record","resource-name":"dr1","content-type":"text/plain","content":"45","dt-expiration":null}'
curl -i "$BASE/app1/channel1/dr1"
curl -i "$BASE/app1" -H 'solid-discovery: data-record'

# Subscription: CREATE / READ / DISCOVER (sem UPDATE)
curl -i -X POST "$BASE/app1/channel1" -H 'Content-Type: application/json' \
  -d '{"res-type":"subscription","resource-name":"sub1","evt":1,"endpoint":"http://localhost:9090/notifications","dt-expiration":null}'
curl -i "$BASE/app1/channel1/subs/sub1"
curl -i "$BASE/app1" -H 'solid-discovery: subscription'

# Confirmar que GET normal não faz listagem de todos os recursos
curl -i "$BASE/app1/channel1"
# Confirmar escopo vazio e status HTTP 200
curl -i "$BASE/app1/channel1/dr1" -H 'solid-discovery: data-record'

# Deletes: ordem descendente-ascendente
curl -i -X DELETE "$BASE/app1/channel1/dr1"
curl -i -X DELETE "$BASE/app1/channel1/subs/sub1"
curl -i -X DELETE "$BASE/app1/channel1"
curl -i -X DELETE "$BASE/app1"
```

## Cobertura adicional necessária no relatório

- `POST` sem `resource-name` ⇒ resposta com nome gerado.
- `POST` com nome duplicado ⇒ resposta com nome diferente válido.
- `POST` inválido sem `res-type`.
- Rejeição de `PUT/PATCH` para `data-record` e `subscription`.
- `evt` 0, 3 ou string ⇒ invalidação conforme §2.4.
- `dt-expiration` passado, futuro, vazio e inválido.
- Recursividade de `solid-discovery` e caminhos de subscrições com `/subs/`.
- Eventos HTTP e MQTT de creation/deletion e formatos de payload.

A secção de comandos cURL da versão final do relatório deve refletir as rotas/códigos/status **efetivamente implementados** e ser executada antes da entrega.
