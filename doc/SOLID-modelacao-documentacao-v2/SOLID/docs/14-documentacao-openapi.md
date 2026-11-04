# 14 — Contrato Swagger/OpenAPI: checklist

## Documentar explicitamente

1. Base HTTP `http://localhost:8765/api/solid` e contexto de deployment.
2. Todos os `POST`, `GET`, `PUT` e `DELETE` suportados na tabela CRUD.
3. O **header** opcional `solid-discovery` nos GET e os valores permitidos.
4. A diferença entre GET regular (DTO único) e GET discovery (`string[]`).
5. `res-type` obrigatório em bodies e os campos JSON com hífen literal.
6. `dt-creation` gerado pelo servidor; `dt-expiration` opcional e formato ISO 8601.
7. `content` string com `content-type` que pode ser JSON/XML/texto.
8. Nota de **imutabilidade** de `data-record`/`subscription` (sem PUT).
9. URI de subscription com `/subs` no GET/DELETE e nas respostas de descoberta.
10. Respostas de criação/atualização com representação completa, sem IDs internos.
11. Códigos de resposta e exemplos de erro consistentes.
12. Exemplos concretos de todos os quatro tipos e discovery, incluindo `200 []`.

## Critério de qualidade

Usar os exemplos cURL de `09-exemplos-curl.md` para verificar se os schemas e os exemplos Swagger refletem o comportamento real. Uma especificação OpenAPI JSON gerada não é, por si só, necessariamente uma interface **Swagger UI**: a interface visual precisa de estar configurada se exigida pela avaliação.
