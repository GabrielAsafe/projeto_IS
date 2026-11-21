
#dotnet run --project middleware --urls http://localhost:8765



curl -i -X GET \
  http://localhost:8765/api/solid/app1/channel1/dr1



curl -i -X DELETE \
  http://localhost:8765/api/solid/app1/channel1/dr1



curl -i -X GET \
  http://localhost:8765/api/solid/app1/channel1/dr1



curl -i -X DELETE \
  http://localhost:8765/api/solid/app1/channel1/dr1



curl -i -X GET \
  http://localhost:8765/api/solid/app1/channel1/subs/sub1



curl -i -X DELETE \
  http://localhost:8765/api/solid/app1/channel1/subs/sub1



curl -i -X GET \
  http://localhost:8765/api/solid/app1/channel1/subs/sub1



curl -i -X DELETE \
  http://localhost:8765/api/solid/app1/channel1/subs/sub1


curl -i -X GET \
  http://localhost:8765/api/solid/app1/channel1 \
  -H "solid-discovery: data-record"

  curl -i -X GET \
  http://localhost:8765/api/solid/app1/channel1 \
  -H "solid-discovery: subscription"