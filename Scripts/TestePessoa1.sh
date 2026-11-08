#precisa estar a rodar em um terminal
#dotnet run --project middleware --urls http://localhost:8765

curl -i -X POST http://localhost:8765/api/solid \
  -H "Content-Type: application/json" \
  -d '{
    "res-type": "application",
    "resource-name": "app-test",
    "resource-desc": "Application for testing"
  }'



curl -i -X POST http://localhost:8765/api/solid \
  -H "Content-Type: application/json" \
  -d '{
    "res-type": "application",
    "resource-desc": "Application without name"
  }'



curl -i -X POST http://localhost:8765/api/solid \
  -H "Content-Type: application/json" \
  -d '{
    "res-type": "application",
    "resource-name": "app-test",
    "resource-desc": "Duplicated application name"
  }'


curl -i http://localhost:8765/api/solid \
  -H "solid-discovery: application"