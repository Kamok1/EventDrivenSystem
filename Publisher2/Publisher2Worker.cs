using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Publisher2.Data;
using Microsoft.Extensions.Logging;

namespace Publisher2;

public class Publisher2Worker(
    IEventPublisher publisher,
    IServiceScopeFactory scopeFactory,
    ILogger<Publisher2Worker> logger) : BackgroundService
{
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Publisher2] Uruchomiono Publisher2Worker. Metoda: ExecuteAsync");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delayMs = _random.Next(2000, 8000);
            logger.LogInformation("[Publisher2] Oczekiwanie {Delay}ms przed wysłaniem Typ2Event", delayMs);

            await Task.Delay(delayMs, stoppingToken);

            var @event = new Typ2Event
            {
                SourceService = "Publisher2",
                Data = $"Losowe dane z Publisher2 o {DateTime.UtcNow:HH:mm:ss.fff}"
            };

            logger.LogInformation("[Publisher2] Generowanie Typ2Event (Id={EventId})", @event.Id);

            publisher.Publish(@event);

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Publisher2DbContext>();

            dbContext.PublishedEvents.Add(new PublishedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ2Event),
                Data = @event.Data,
                PublishedAt = @event.CreatedAt
            });

            await dbContext.SaveChangesAsync(stoppingToken);
            logger.LogInformation("[Publisher2] Zapisano log zdarzenia do bazy danych");
        }
    }
}
