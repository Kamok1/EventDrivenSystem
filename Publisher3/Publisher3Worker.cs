using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Publisher3.Data;
using Microsoft.Extensions.Logging;

namespace Publisher3;

public class Publisher3Worker(
    IEventPublisher publisher,
    IServiceScopeFactory scopeFactory,
    ILogger<Publisher3Worker> logger) : BackgroundService
{
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Publisher3] Uruchomiono Publisher3Worker. Metoda: ExecuteAsync");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delayMs = _random.Next(3000, 10000);
            logger.LogInformation("[Publisher3] Oczekiwanie {Delay}ms przed wysłaniem Typ3Event", delayMs);

            await Task.Delay(delayMs, stoppingToken);

            var @event = new Typ3Event
            {
                SourceService = "Publisher3",
                Data = $"Losowe dane z Publisher3 o {DateTime.UtcNow:HH:mm:ss.fff}"
            };

            logger.LogInformation("[Publisher3] Generowanie Typ3Event (Id={EventId})", @event.Id);

            publisher.Publish(@event);

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Publisher3DbContext>();

            dbContext.PublishedEvents.Add(new PublishedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ3Event),
                Data = @event.Data,
                PublishedAt = @event.CreatedAt
            });

            await dbContext.SaveChangesAsync(stoppingToken);
            logger.LogInformation("[Publisher3] Zapisano log zdarzenia do bazy danych");
        }
    }
}
