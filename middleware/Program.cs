using middleware.Services;
using middleware.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<SolidDatabase>();
builder.Services.AddHttpClient<NotificationService>();
builder.Services.AddHostedService<ExpirationService>();
builder.Services.AddSingleton<MqttService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/dev/receive", async (HttpRequest request) =>
    {
        using var reader = new StreamReader(request.Body);
        string body = await reader.ReadToEndAsync();

        Console.WriteLine("Notification received:");
        Console.WriteLine(body);

        return Results.Ok();
    });

    app.MapPost("/dev/test-event",
        async (NotificationService notifications) =>
    {
        var record = new middleware.Models.DataRecord
        {
            ChannelId = 1,
            ResourceName = "dr-test",
            ContentType = "text/plain",
            Content = "25",
            DtCreation = DateTime.UtcNow.ToString(
                "yyyy-MM-ddTHH:mm:ss"
            )
        };

        await notifications.NotifyAsync(
            record.ChannelId,
            1,
            record,
            "/api/solid/app1/channel1/dr-test"
        );

        return Results.Ok();
    });
}



app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();


