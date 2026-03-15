using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Consumer3.Data;
using Microsoft.Extensions.Logging;

namespace Consumer3;

public class Consumer3Worker(
    IEventConsumer consumer,
    IEventPublisher publisher,
    IServiceScopeFactory scopeFactory,
    ILogger<Consumer3Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Consumer3] Uruchomiono Consumer3Worker. Metoda: ExecuteAsync");
        logger.LogInformation("[Consumer3] Subskrypcja na kolejkę Typ3Event (z automatycznym przekazaniem na Typ4Event)");

        consumer.Subscribe<Typ3Event>(@event =>
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Consumer3DbContext>();

            logger.LogInformation(
                "[Consumer3] Odebrano Typ3Event (Id={EventId}, Source={Source}, Data={Data})",
                @event.Id, @event.SourceService, @event.Data);

            dbContext.ReceivedEvents.Add(new ReceivedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ3Event),
                Data = @event.Data,
                SourceService = @event.SourceService,
                ReceivedAt = DateTime.UtcNow
            });

            var typ4Event = new Typ4Event
            {
                SourceService = "Consumer3",
                Data = $"Przekazano z Typ3Event (OriginalId={@event.Id}): {@event.Data}"
            };

            logger.LogInformation(
                "[Consumer3] Generowanie Typ4Event (Id={EventId}) na podstawie Typ3Event (OriginalId={OriginalId})",
                typ4Event.Id, @event.Id);

            publisher.Publish(typ4Event);

            dbContext.ForwardedEvents.Add(new ForwardedEventLog
            {
                OriginalEventId = @event.Id,
                ForwardedEventId = typ4Event.Id,
                ForwardedEventType = nameof(Typ4Event),
                ForwardedAt = DateTime.UtcNow
            });

            dbContext.SaveChanges();
            logger.LogInformation("[Consumer3] Zapisano logi (odbiór + przekazanie) do bazy danych");
        });

        return Task.CompletedTask;
    }
}
