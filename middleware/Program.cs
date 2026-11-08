using middleware.Services;
using middleware.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton<SolidDatabase>();// uma instância por aplicalção mas se fosse scoped seria um por requisição
builder.Services.AddHttpClient<NotificationService>();
builder.Services.AddHostedService<ExpirationService>();
builder.Services.AddSingleton<MqttService>();//uma instância para toda a aplicalção


var app = builder.Build();


app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();


