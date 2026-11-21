# 16 — XML/XSD de registo de notificações (exemplo)

Estes são **exemplos de esquema**, não um XSD oficial do enunciado nem ficheiros produzidos por uma aplicação funcional:

- `../exemplos/valve-notifications.xsd`
- `../exemplos/valve-notifications-example.xml`

O enunciado exige que **todas as notificações recebidas pela Water Valve Application** sejam persistidas em XML e que o XML seja validado contra um XSD criado pela equipa. O modelo apresentado regista evento, URI pública, instante, humidade e um payload textual opcional. Tem intervalos 0..100 e admite creation/deletion.

**Limitação intencional:** Notificações sem humidade válida não são aceites neste esquema, pelo que a equipa precisa de escolher como reter provas de notificação inválida sem violar a regra de persistir todas as recebidas. A estrutura real deve ser alinhada com o payload efetivo do middleware, que não está fixado pelo enunciado.
