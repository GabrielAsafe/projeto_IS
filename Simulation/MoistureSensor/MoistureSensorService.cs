namespace MoistureSensor;

//using middleware.Models.datarecord;

public class Worker(ILogger<Worker> logger, IConfiguration config) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var middlewareUrl = config["MiddlewareUrl"] ?? "http://localhost:8765";
        var client = new SolidClient(middlewareUrl);
        
        await client.EnsureResourcesExistAsync();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        int humidity = Random.Shared.Next(0, 101);

        while (await timer.WaitForNextTickAsync(stoppingToken) )
        {
            if(!stoppingToken.IsCancellationRequested){
                
                humidity = Math.Clamp(humidity + Random.Shared.Next(-3, 4), 0, 100);
                logger.LogInformation("Humidity: {h}%", humidity);
                await client.PublishHumidityAsync(humidity);

                // post datarecord para o middleware

            }

        }
    }

}
