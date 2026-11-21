# Árvore de recursos e URIs

> **Modelo proposto**, não implementação concluída.

```mermaid
flowchart TD
    Root["/api/solid"] --> App["/app1 — application"]
    App --> Ch["/app1/channel1 — channel"]
    Ch --> Rec["/app1/channel1/dr1 — data-record"]
    Ch -.-> SubNode["/app1/channel1/subs — virtual"]
    SubNode --> Sub["/app1/channel1/subs/sub1 — subscription"]
    style SubNode stroke-dasharray: 5 5
```

`subs` é um segmento de endereçamento **virtual, não persistido**. A criação do record e da subscrição é feita com `POST /api/solid/app1/channel1`, distinguida pelo campo `res-type`. A figura apresenta um exemplo mínimo, não a cardinalidade global completa.
