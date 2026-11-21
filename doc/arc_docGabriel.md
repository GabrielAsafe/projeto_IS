# Podcast: SOLID — Como funciona o projeto de Integração de Sistemas

Episódio especial: A arquitetura completa do SOLID, com foco nas notificações HTTP, MQTT, eventos e expiração automática — a tua parte do projeto.

Imagina que estamos a conversar sobre o projeto, sem estar constantemente a olhar para código. O objetivo é que, no final, consigas perceber o que cada módulo faz, como tudo se liga e, principalmente, porque é que a tua parte é fundamental para o sistema funcionar.

## Capítulo 1 — Afinal, o que estamos a construir?

Imagina uma cidade inteligente. Nessa cidade existem sensores de temperatura, sensores de humidade, sistemas de rega, semáforos e muitos outros dispositivos.

Todos estes dispositivos produzem informação. Mas há um problema: cada um pode utilizar tecnologias diferentes e comunicar de maneiras diferentes.

Se quisermos que um sensor comunique com uma válvula, temos de programar essa comunicação. Se depois quisermos ligar o mesmo sensor a outra aplicação, provavelmente teremos de criar outra integração.

É precisamente este problema que o vosso projeto procura resolver.

O professor pediu-vos para construir o SOLID — Service-Oriented Layer for Interoperable Data.

O SOLID funciona como um intermediário entre aplicações.

Em vez de as aplicações comunicarem diretamente umas com as outras, comunicam através do middleware.

Pensa no middleware como uma central de distribuição de informação.

Uma aplicação envia dados para essa central, e outras aplicações podem consultar esses dados ou ser notificadas quando acontece alguma coisa.

No vosso projeto, vão demonstrar esta arquitetura através de um sistema de rega automática.

Sensor de humidade

Produz medições a cada segundo

HTTP POST

SOLID Middleware

Recebe, armazena e distribui eventos

Notificação HTTP ou MQTT

Válvula de rega

Decide se deve ligar ou desligar

O sensor não precisa de conhecer a válvula. A válvula não precisa de consultar constantemente o sensor.

Ambos só precisam de conhecer o SOLID.

## Capítulo 2 — Os quatro recursos do middleware

Antes de falarmos da tua parte, precisamos de compreender a estrutura dos dados.

O SOLID trabalha com quatro tipos de recursos.

Application é o nível superior. Representa uma aplicação registada no sistema. Por exemplo, uma aplicação chamada `app1` ou uma aplicação de sensores de humidade.

Channel é um canal que pertence a uma aplicação. Serve para organizar informação relacionada. Uma aplicação pode ter vários canais.

Data-record é um registo de dados. Imagina que o sensor mediu 25% de humidade. Essa medição pode ser guardada como um `data-record`.

Subscription é uma regra que diz ao middleware: "Quando acontecer determinado evento neste canal, avisa-me".

E esta última parte é essencial para perceberes o teu trabalho.

Imagina esta estrutura:

```
SOLID
│
└── app1
    │
    └── channel1
        │
        ├── dr1
        ├── dr2
        ├── dr3
        │
        └── subs
            ├── sub1
            └── sub2
```

Aqui, `dr1`, `dr2` e `dr3` são registos de dados.

Já `sub1` e `sub2` são subscrições.

O `subs` é apenas um nó virtual utilizado nos URLs, não é uma tabela ou um recurso persistente adicional.

Por exemplo, para consultar uma subscrição:

`GET /api/solid/app1/channel1/subs/sub1`

Uma distinção fundamental: um `data-record` guarda informação; uma `subscription` define quem deve ser avisado quando a informação muda.

## Capítulo 3 — A responsabilidade da Pessoa 1: API e estrutura

A primeira pessoa do grupo é responsável pelas fundações do sistema.

Tem de configurar o projeto .NET 10, estabelecer a ligação ao SQLite e implementar as operações sobre aplicações e canais.

É aqui que entram os Controllers.

Os Controllers recebem os pedidos HTTP e determinam o que fazer.

Imagina que chega:

```
POST /api/solid
Content-Type: application/json
```

Com um JSON que representa uma aplicação.

O Controller recebe o pedido, valida os dados e utiliza o código de acesso à base de dados para guardar a aplicação.

O mesmo princípio aplica-se a consultas, atualizações e eliminações.

Também fica desta parte a documentação da API através de Swagger/OpenAPI.

A base de dados tem de guardar os quatro tipos de recursos, mas os IDs internos do SQLite não podem aparecer nos URLs ou nas respostas JSON.

A Pessoa 1 está, portanto, a preparar a infraestrutura que todos os restantes módulos vão utilizar.

## Capítulo 4 — A responsabilidade da Pessoa 2: recursos e Discovery

A segunda pessoa continua o trabalho sobre a API, mas concentra-se nos `data-records` e nas `subscriptions`.

É ela que implementa, por exemplo, a criação e eliminação de medições.

Quando uma aplicação faz:

```
POST /api/solid/app1/channel1
```

Com `res-type` igual a `data-record`, a API deve criar um novo registo naquele canal.

A Pessoa 2 também implementa a criação de subscrições e as operações de Discovery.

Discovery não é a mesma coisa que uma notificação.

Discovery é quando o cliente pergunta ativamente ao middleware quais os recursos existentes de determinado tipo.

Por exemplo:

```
GET /api/solid/app1
solid-discovery: data-record
```

O servidor devolve um array JSON com os caminhos dos `data-records` encontrados dentro de `app1`.

Já uma notificação funciona ao contrário: é o middleware que contacta a aplicação quando acontece um evento.

E é aqui que entramos na tua responsabilidade.

# Capítulo 5 — Pessoa 3: o coração do sistema de eventos

Esta é a tua parte.

És responsável por três grandes funcionalidades: detetar e comunicar eventos, enviar notificações através de HTTP ou MQTT e remover automaticamente recursos expirados.

O teu código transforma o middleware de uma simples API com base de dados num sistema capaz de reagir a acontecimentos.

Vamos imaginar uma situação concreta.

Existe uma subscription chamada `sub1`, associada a `channel1`.

Essa subscription diz:

"Quero receber uma notificação sempre que um novo data-record for criado neste canal."

O valor `evt` define aquilo que interessa ao subscritor.

| `evt` | Significado                                 |
| ----- | ------------------------------------------- |
| `1`   | Notificar quando um data-record é criado    |
| `2`   | Notificar quando um data-record é eliminado |

O enunciado final exige apenas estes dois valores. Se alguém quiser receber os dois tipos de evento, pode criar duas subscriptions.

## Parte A — Como nasce uma notificação?

Imagina que o sensor acabou de medir 25% de humidade.

A aplicação do sensor envia um POST para o middleware.

A Pessoa 2 recebe o pedido através do Controller, valida-o e guarda o `data-record` na base de dados.

Agora acontece algo importante.

Guardar o registo não é suficiente.

O middleware tem de verificar se existe alguma aplicação interessada em saber que aquele registo foi criado.

É aqui que o teu `NotificationService` entra.

1. O Controller recebe o POST

   O sensor publica um novo `data-record` com o valor 25.
2. A base de dados guarda o registo

   O SQLite confirma que o registo foi criado.
3. O Controller chama o teu NotificationService

   Informa que aconteceu um evento de criação no canal.
4. O NotificationService consulta as subscriptions

   Procura as que pertencem ao canal, têm `evt = 1` e não estão expiradas.
5. O NotificationService constrói a mensagem

   Inclui o evento, o recurso afetado e o caminho completo.
6. O NotificationService envia a mensagem

   Utiliza HTTP ou MQTT, dependendo do endpoint da subscription.

Agora percebes porque estávamos a falar de combinar uma interface com a Pessoa 2.

O teu trabalho não é necessariamente guardar o `data-record`.

É reagir depois de ele ter sido guardado com sucesso.

Por isso, podes criar este contrato:

```
public async Task NotifyAsync(
    int channelId,
    int eventType,
    DataRecord record,
    string resourcePath)
{
    // Encontrar subscriptions
    // Criar mensagem
    // Enviar notificações
}
```

A Pessoa 2 chama o teu método depois da criação.

E também o chama quando um registo é eliminado com sucesso.

O `channelId` ajuda-te a encontrar as subscriptions certas. O `eventType` diz se foi uma criação ou eliminação. O `record` contém os dados que foram afetados. E o `resourcePath` identifica onde aquele recurso existia.

O importante é que o teu método não precisa de saber como foi feito o POST ou o DELETE.

Ele só precisa de receber a informação necessária para enviar o evento.

## Parte B — Como encontrar quem quer receber notificações?

Imagina que existem cem subscriptions no middleware.

Algumas pertencem a outros canais. Outras estão interessadas em eliminações, não em criações. E outras já expiraram.

Não podes notificar todas indiscriminadamente.

Por isso, tens o método `GetInterestedSubscriptions()`.

A query principal é:

```
SELECT *
FROM subscriptions
WHERE channel_id = @ChannelId
  AND evt = @EventType
  AND (
      expires_at IS NULL
      OR trim(expires_at) = ''
      OR datetime(expires_at) > datetime('now')
  );
```

Esta consulta está a perguntar:

"Quais são as subscriptions deste canal, interessadas neste evento, que ainda estão válidas?"

Se encontrares zero subscriptions, não envias nada.

Se encontrares cinco, tens de entregar a notificação aos cinco destinos, respeitando o protocolo de cada um.

E aqui começa a segunda grande responsabilidade do teu serviço.

## Capítulo 6 — HTTP versus MQTT

Tens de suportar duas formas de enviar notificações.

São duas tecnologias com o mesmo objetivo, mas com mecanismos diferentes.

### Notificações HTTP

Imagina que uma aplicação quer receber eventos através de HTTP.

Ela regista uma subscription cujo endpoint é:

`http://localhost:9000/notifications`

Quando acontece um evento, o teu middleware faz um HTTP POST para esse endereço.

O corpo do pedido pode ser, por exemplo:

```
{
  "event": "creation",
  "resource": {
    "res-type": "data-record",
    "resource-name": "dr1",
    "content-type": "text/plain",
    "content": "25",
    "dt-creation": "2026-11-02T10:00:00",
    "dt-expiration": ""
  },
  "path": "/api/solid/app1/channel1/dr1"
}
```

Este é um formato proposto para a mensagem. O enunciado exige essas três informações, mas não determina os nomes exatos `event`, `resource` e `path`.

Em C#, podes utilizar `HttpClient` para efetuar o POST.

A aplicação de destino tem de estar preparada para receber esses pedidos.

Se o destino devolver um erro, o teu serviço deve registar a falha e continuar a processar os restantes destinatários.

É importante não permitir que uma aplicação desligada impeça todas as outras de receber notificações.

### Notificações MQTT

Agora imagina uma situação com muitos sensores e dispositivos.

Em vez de o middleware enviar diretamente um HTTP POST para cada aplicação, pode publicar mensagens num broker MQTT.

O broker funciona como um intermediário de mensagens.

O teu middleware liga-se ao broker e publica uma notificação num tópico.

Por exemplo:

`api/solid/app1/channel1`

As aplicações interessadas subscrevem esse tópico no broker MQTT e recebem as mensagens publicadas.

NotificationService

Publica uma notificação MQTT

Tópico: api/solid/app1/channel1

MQTT Broker

Distribui a mensagem aos clientes subscritos

Aplicação A

Aplicação B

Aplicação C

A diferença fundamental é esta.

Com HTTP, o teu middleware contacta diretamente o endpoint de destino.

Com MQTT, o middleware publica uma mensagem no broker, que a distribui aos clientes interessados no tópico.

E há uma regra muito importante do vosso enunciado:

Nas subscriptions MQTT, o campo `endpoint` deve guardar o endereço do broker MQTT, não o tópico.

O tópico é determinado em tempo de execução, a partir do caminho completo do canal.

No teu `NotificationService`, deves portanto analisar o `endpoint` da subscription, e não o `resourcePath` do data-record, para decidir o mecanismo de transporte.

Também deves combinar com o grupo uma convenção clara para distinguir endereços HTTP e MQTT.

## Capítulo 7 — Os eventos de criação e eliminação

Até agora falámos sobretudo da criação.

Mas o teu middleware também tem de avisar quando um `data-record` é eliminado.

Imagina que tens este registo na base de dados:

`/api/solid/app1/channel1/dr1`

O cliente faz:

```
DELETE /api/solid/app1/channel1/dr1
```

A Pessoa 2 encontra o recurso e elimina-o.

O teu `NotificationService` deve receber:

- O tipo de evento `2`, que corresponde a deletion.
- Os dados completos do registo eliminado.
- O caminho onde o registo existia.

Existe aqui um detalhe técnico importante.

Para enviar a representação do recurso eliminado, é necessário recuperá-la antes de o apagar.

Mas a notificação só deve ser desencadeada depois de confirmar que a eliminação foi concluída com sucesso.

Ou seja, o fluxo correto é:

1\. Consultar e guardar os dados do recurso

2\. Eliminar o recurso da base de dados

3\. Confirmar que a eliminação teve sucesso

4\. Enviar a notificação aos interessados

Também deves impedir notificações duplicadas. Se o recurso já não existir e o DELETE não eliminar nada, não há um novo evento de eliminação para comunicar.

# Capítulo 8 — A expiração automática: a tua segunda grande responsabilidade

Agora vamos falar do `ExpirationService`.

Este serviço resolve um problema diferente.

Até agora, todos os eventos surgiam de operações executadas pelos clientes.

Mas imagina que uma aplicação cria um `data-record` com uma data de expiração.

O recurso deve desaparecer automaticamente quando essa data for ultrapassada, mesmo que nenhum cliente faça um DELETE.

O mesmo se aplica às subscriptions.

Por exemplo:

```
{
  "res-type": "data-record",
  "resource-name": "dr1",
  "content-type": "text/plain",
  "content": "25",
  "dt-creation": "2026-11-02T10:00:00",
  "dt-expiration": "2026-11-02T10:05:00"
}
```

Este registo expira às 10:05.

Quem é responsável por verificar isto?

Não é o Controller.

Não é o sensor.

Não é a válvula.

É o teu `ExpirationService`.

### Como funciona o BackgroundService?

Em ASP.NET Core, podes criar um serviço que é executado em segundo plano enquanto o middleware está ativo.

O teu `ExpirationService` herda de `BackgroundService`.

Quando o servidor arranca, o serviço começa a executar o método `ExecuteAsync()`.

Podes usar um `PeriodicTimer` para verificar periodicamente se há recursos expirados.

A lógica é aproximadamente esta:

```
protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
{
    using var timer =
        new PeriodicTimer(TimeSpan.FromSeconds(5));

    while (await timer.WaitForNextTickAsync(stoppingToken))
    {
        // Encontrar recursos expirados

        // Eliminar subscriptions expiradas

        // Eliminar data-records expirados

        // Notificar eliminações de data-records
    }
}
```

Este é apenas o esqueleto; na implementação real precisas de tratar exceções e o encerramento do serviço.

E agora vem uma questão interessante.

### O que acontece quando um data-record expira?

Imagina que `dr1` expira.

O teu `ExpirationService` encontra o registo, recupera os dados e elimina-o.

Esta eliminação também deve desencadear o mecanismo de notificações de eliminação.

Ou seja, o `ExpirationService` pode chamar o `NotificationService`.

É uma interação entre dois módulos que tu próprio desenvolves.

ExpirationService

Encontra um DataRecord expirado

SolidDatabase

Elimina o recurso do SQLite

NotificationService

Envia evento de eliminação (evt=2)

Não precisas de enviar notificações pela eliminação de subscriptions, porque o enunciado define os eventos de notificação para a criação e eliminação de `data-records`.

Além disso, as subscriptions expiradas não devem receber novas notificações. Por isso, o teu serviço deve eliminá-las ou, pelo menos, garantir que são ignoradas nas consultas de subscriptions ativas.

O objetivo é não deixar recursos com validade ultrapassada permanentemente na base de dados.

### Mas o professor não proíbe polling?

Sim, mas é importante perceber o contexto.

O professor proíbe que a aplicação da válvula esteja constantemente a consultar o middleware para procurar novas medições.

Já o middleware tem de garantir a eliminação automática dos recursos expirados.

Por isso, um `BackgroundService` com verificações periódicas de expiração é uma solução adequada.

Não estás a violar o princípio event-driven da comunicação do sensor com a válvula.

## Capítulo 9 — A responsabilidade da Pessoa 4: sensor e válvula

Agora que compreendemos o teu trabalho, vamos perceber como ele é utilizado pelos simuladores.

A quarta pessoa do grupo tem de desenvolver duas aplicações independentes.

A primeira simula um sensor de humidade.

De segundo a segundo, gera um valor entre 0 e 100.

Esse valor é enviado para o middleware através de um novo `data-record`.

A segunda aplicação simula uma válvula de rega.

O utilizador configura um limite, por exemplo 40%.

Quando recebe uma nova medição, a válvula aplica esta regra:

Simulação da válvula

ON — Rega ligada

Humidade recebida

25%

Limite configurado

40%

25% < 40%

A humidade está abaixo do limite. A válvula liga para regar.

Além disso, a aplicação da válvula deve guardar todas as notificações recebidas num ficheiro XML e validar esse ficheiro através de um XSD desenvolvido pelo grupo.

E repara como a tua parte é essencial.

Sem o `NotificationService`, a aplicação do sensor conseguiria enviar dados para o middleware, mas a válvula não seria informada automaticamente de novas medições.

O sistema deixaria de demonstrar uma das funcionalidades principais do projeto.

# Capítulo 10 — Vamos acompanhar uma operação do início ao fim

Agora imagina o vosso projeto totalmente implementado.

O servidor arranca na porta `8765`.

A base de dados contém a aplicação `app1`, o canal `channel1` e as respetivas subscriptions.

A aplicação do sensor começa a funcionar.

EXEMPLO DE EXECUÇÃO — HUMIDADE DE 25%, LIMITE DE 40%

10:00:00

O sensor gera 25%

Publica uma nova medição através da API.

10:00:00

A API guarda dr1

A Pessoa 2 confirma a criação no SQLite.

10:00:00

O teu NotificationService é chamado

Recebe channelId, evento 1, recurso e caminho.

10:00:00

Pesquisa subscriptions

Encontra uma subscription HTTP interessada em criações.

10:00:00

Envia uma notificação HTTP

A aplicação da válvula recebe os dados de dr1.

10:00:00

A válvula liga

25% é inferior ao limite de 40%. Guarda a notificação em XML.

10:05:00

O DataRecord expira

O ExpirationService deteta e remove dr1.

10:05:00

É gerado o evento de eliminação

O NotificationService procura subscriptions com evt=2 e notifica os destinos interessados.

Tempos ilustrativos. A expiração é processada quando o serviço de background efetuar a verificação seguinte.

A válvula reage à medição através da subscrição de criação. A notificação de eliminação só será recebida pelas aplicações com subscriptions de `evt=2`.

Esta é a razão pela qual o middleware suporta dois tipos de eventos.

## Capítulo 11 — Como o teu código se encaixa na arquitetura

Agora vamos relacionar tudo o que explicámos com os ficheiros que já começaste a desenvolver.

```
middleware/
│
├── Controllers/
│   ├── ApplicationsController.cs       Pessoa 1
│   ├── ChannelsController.cs           Pessoa 1
│   ├── DataRecordsController.cs        Pessoa 2
│   └── SubscriptionsController.cs      Pessoa 2
│
├── Models/
│   ├── Application.cs
│   ├── Channel.cs
│   ├── DataRecord.cs
│   └── Subscription.cs
│
├── Database/
│   └── SolidDatabase.cs                Acesso SQLite
│
├── Services/
│   ├── NotificationService.cs          TUA PARTE
│   └── ExpirationService.cs            TUA PARTE
│
└── Program.cs
```

O `NotificationService` recebe eventos, consulta subscriptions e envia mensagens. Deve ser chamado quando uma operação sobre um `data-record` tem sucesso.

O `ExpirationService` corre automaticamente enquanto o middleware está ativo. Verifica expirações, remove recursos e comunica eliminações de `data-records` ao `NotificationService`.

O `SolidDatabase` disponibiliza as operações SQL necessárias. No vosso caso, estão a utilizar `Microsoft.Data.Sqlite`, tal como nas aulas, sem Entity Framework.

O `Program.cs` regista os serviços para que o ASP.NET Core possa criá-los e utilizá-los.

A tua implementação deve separar estas responsabilidades. Não deves colocar queries SQL, construção de JSON, envio HTTP, MQTT e controlo do temporizador tudo num único método gigante.

## Capítulo 12 — O que precisas realmente dos teus colegas?

A boa notícia é que consegues desenvolver quase toda a tua parte independentemente.

Da Pessoa 1, precisas sobretudo de uma base SQLite funcional e das tabelas já definidas. Não precisas de esperar pelo Swagger ou por todos os Controllers.

Da Pessoa 2, precisas de combinar um contrato: quando houver criação ou eliminação bem-sucedida de um `data-record`, o código dela deverá chamar o teu `NotificationService`.

Enquanto essa integração não existir, podes simular os eventos, como já tinhas pensado com o `MockDataRecordService`.

Também podes inserir subscriptions de teste diretamente no SQLite. Assim testas o teu serviço com dados reais, mesmo antes de existir o CRUD completo das subscriptions.

O ideal é teres primeiro uma subscription HTTP de criação, depois uma de eliminação e, mais tarde, subscriptions MQTT.

Para a expiração, podes inserir um `data-record` com uma data próxima e verificar se desaparece automaticamente e se o evento de eliminação é enviado.

Um último detalhe importante: a eliminação automática e a notificação são operações diferentes. Se a notificação falhar por um problema de rede, isso não deve fazer o middleware pensar que o recurso continua por eliminar. Registar as falhas e decidir como as recuperar faz parte de tornar o serviço fiável.

## Final do episódio — O que deves conseguir explicar na discussão do projeto

Quando o professor te perguntar qual foi a tua contribuição, deves conseguir responder algo deste género:

> Fiquei responsável pelo sistema de eventos e notificações do middleware SOLID e pela expiração automática dos recursos.
>
> Implementei um serviço que recebe eventos de criação e eliminação de data-records, consulta as subscriptions ativas associadas ao canal e envia notificações aos destinos interessados, através de HTTP ou MQTT.
>
> Também desenvolvi um BackgroundService que verifica as datas de expiração de data-records e subscriptions, remove automaticamente os recursos expirados e desencadeia notificações quando um data-record é eliminado.
>
> Estes serviços permitem que aplicações externas reajam às alterações dos dados sem precisarem de consultar continuamente a API.

E essa é a essência da tua responsabilidade.

A Pessoa 1 constrói a infraestrutura. A Pessoa 2 permite guardar e manipular os recursos. Tu fazes o middleware reagir aos acontecimentos e comunicar com o exterior. A Pessoa 4 demonstra tudo isso a funcionar com o sensor e a válvula.

Se tiveres bem presentes estas três perguntas — o que aconteceu, quem quer saber e como entregar a informação — já compreendeste a maior parte da lógica do teu `NotificationService`.

E, para o `ExpirationService`, a pergunta é: que recursos já ultrapassaram a validade, como os removo corretamente e que eventos essa remoção deve gerar?

Essas são as duas ideias centrais que deves dominar para implementar e defender a tua parte do projeto SOLID.