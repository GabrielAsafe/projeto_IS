namespace MoistureSensor;

using middleware.Models.datarecord;
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        int humidity = Random.Shared.Next(0, 101);

        while (await timer.WaitForNextTickAsync(stoppingToken) )
        {
            if(!stoppingToken.IsCancellationRequested){
                
                if (logger.IsEnabled(LogLevel.Information)){
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                humidity += Random.Shared.Next(0, 3);

                // post datarecord para o middleware

            }

        }
    }

}
