git init

mkdir SOLID
cd SOLID

dotnet new sln -n SOLID

dotnet new webapi -n middleware

dotnet sln SOLID.slnx add middleware/middleware.csproj
cd middleware
dotnet build
dotnet run

-------

em middleware.http e launchSettings mudei a porta para 8765



-------

já existe proposta da bd e seed, e já fiz o json


------
adicionar a base de dados está na ficha 3 ex 4. Creating a RESTful Web Service with Database Access



-----
perguntei ao gpt e faltavam coisas 

dotnet add package Microsoft.Data.Sqlite

mkdir Data
cd Data
# para criar
sqlite3 database.db < schema.sql 
# para garintir que criou 
sqlite3 database.db ".tables"
# para inserir os dados seed

# full creation
cd ~/Desktop/projetos/IS/SOLID
rm -rf Data/database.db
touch Data/database.db
sqlite3 Data/database.db < Data/schema.sql 
sqlite3 Data/database.db ".tables"
sqlite3 Data/database.db < Data/seed.sql
sqlite3 Data/database.db "SELECT * FROM applications;"

-------
depois no program temos que adicionar duas linhas

builder.Services.AddOpenApi();
*builder.Services.AddControllers();

var app = builder.Build();
*app.MapControllers();


----------------

para ligar na database precisa do solidDatabase que é a classe que liga ao sqlite. 
 código é boilerplate da net e tem que adicionar isso no program.cs antes do build 
 
builder.Services.AddSingleton<SolidDatabase>();


-----------------
para o projeto vamos precisar disso 
mkdir Models
mkdir Services

touch Controllers/ApplicationsController.cs
touch Controllers/ChannelsController.cs
touch Controllers/DataRecordsController.cs
touch Controllers/SubscriptionsController.cs
touch Models/Application.cs
touch Models/Channel.cs
touch Models/DataRecord.cs
touch Models/Subscription.cs

touch Services/NotificationService.cs
touch Services/ExpirationService.cs
   
--------------------------------
o primeiro passo é ter a db a entregar os dados que preciso

depois coloquei o contrato da bd na classe data record

criei MockDataRecordService, IDataRecordService

para poder testar precisamos da consulta na base de dados 

agora vou trabalhar no notificationService

builder.Services.AddSingleton<SolidDatabase>();
builder.Services.AddScoped<NotificationService>();



-------

para o mosquitto

cd middleware
dotnet add package MQTTnet

adicionar o import na classe 
using MQTTnet;

worksheet 6

------------
Integração MQTT

1/9

[x] Manter o teste HTTP que já funciona
[ ] Adaptar MqttService para receber o broker como parâmetro
[ ] Registar MqttService no Program.cs
[ ] Integrar MQTT no NotificationService
[ ] Criar uma subscription MQTT na base de dados
[ ] Iniciar Mosquitto e um consumidor de teste
[ ] Confirmar a receção do JSON por MQTT
[ ] Testar também evt=2 através de MQTT
[ ] Integrar os eventos com o CRUD real da Pessoa 2