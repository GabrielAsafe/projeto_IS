namespace middleware.Services;

using middleware.Models;

public class ExpirationService : BackgroundService
{
    private readonly SolidDatabase database;
    private readonly NotificationService notifications;
    private readonly ILogger<ExpirationService> logger;

    public ExpirationService(
        SolidDatabase database,
        NotificationService notifications,
        ILogger<ExpirationService> logger)
    {
        this.database = database;
        this.notifications = notifications;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 5 segundos para testes
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(5)
        );

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    // 1. Apagar subscriptions expiradas
                    int removed = database.DeleteExpiredSubscriptions();

                    logger.LogInformation(
                        "Removed {Count} expired subscriptions",
                        removed
                    );

                    // 2. Procurar DataRecords expirados
                    var records = database.GetExpiredDataRecords();

                    foreach (var (record, path) in records)
                    {
                        // 3. Apagar o DataRecord
                        bool deleted =
                            database.DeleteExpiredDataRecord(record.Id);

                        if (!deleted)
                            continue;

                        // 4. Notificar a eliminação
                        await notifications.NotifyAsync(
                            record.ChannelId,
                            2,
                            record,
                            path,
                            stoppingToken
                        );
                    }
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Error processing expired resources"
                    );
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Encerramento normal do serviço.
        }
    }
}