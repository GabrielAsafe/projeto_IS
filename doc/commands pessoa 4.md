# pessoa 4


# Criar a solução (apenas se ainda não existir)
dotnet new sln -n SOLID

# Criar as aplicações
dotnet new webapi -n WaterValve -o Simulation/WaterValve --framework net10.0

dotnet new worker -n MoistureSensor -o Simulation/MoistureSensor --framework net10.0

# Adicionar ambos os projetos à solução
dotnet sln SOLID.slnx add Simulation/WaterValve/WaterValve.csproj

dotnet sln SOLID.slnx add Simulation/MoistureSensor/MoistureSensor.csproj

# Criar os ficheiros adicionais
mv Simulation/MoistureSensor/Worker.cs Simulation/MoistureSensor/MoistureSensorService.cs

touch Simulation/MoistureSensor/SolidClient.cs

touch Simulation/WaterValve/ValveService.cs
touch Simulation/WaterValve/NotificationHandler.cs
touch Simulation/WaterValve/XmlNotificationStorage.cs

mkdir -p Simulation/Schemas
touch Simulation/Schemas/notifications.xsd

# Compilar a solução
dotnet build SOLID.slnx
