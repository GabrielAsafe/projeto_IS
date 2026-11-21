using WaterValve;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Register services
builder.Services.AddSingleton<ValveService>(sp =>
    new ValveService { Threshold = 40 });

builder.Services.AddSingleton<XmlNotificationStorage>(sp =>
{
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "notifications.xml");
    // XSD sijaitsee Schemas-kansiossa projektin juuressa
    var xsdPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory,
        "..", "..", "..", "..", "Schemas", "notifications.xsd"));
    return new XmlNotificationStorage(xmlPath, xsdPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// The middleware sends notifications to this endpoint.
app.MapPost("/notify", async (HttpContext ctx,
                               ValveService valve,
                               XmlNotificationStorage storage) =>
{
    await NotificationHandler.HandleAsync(ctx, valve, storage);
});

// Status endpoint for testing purposes
app.MapGet("/status", (ValveService valve) =>
    Results.Ok(new { valveOpen = valve.IsOpen, threshold = valve.Threshold }));

app.Run();