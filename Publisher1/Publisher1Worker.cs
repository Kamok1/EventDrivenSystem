using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Publisher1.Data;
using Microsoft.Extensions.Logging;

namespace Publisher1;

public class Publisher1Worker(
    IEventPublisher publisher,
    IServiceScopeFactory scopeFactory,
    ILogger<Publisher1Worker> logger,
    string instanceName) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[{Instance}] Uruchomiono Publisher1Worker. Metoda: ExecuteAsync", instanceName);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var @event = new Typ1Event
            {
                SourceService = instanceName,
                Data = $"Dane z {instanceName} o {DateTime.UtcNow:HH:mm:ss.fff}"
            };

            logger.LogInformation("[{Instance}] Generowanie Typ1Event (Id={EventId})", instanceName, @event.Id);

            publisher.Publish(@event);

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Publisher1DbContext>();

            dbContext.PublishedEvents.Add(new PublishedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ1Event),
                Data = @event.Data,
                PublishedAt = @event.CreatedAt
            });

            await dbContext.SaveChangesAsync(stoppingToken);

            logger.LogInformation("[{Instance}] Zapisano log zdarzenia do bazy danych", instanceName);
        }
    }
}
