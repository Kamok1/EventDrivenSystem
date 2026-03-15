using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Consumer2.Data;
using Microsoft.Extensions.Logging;

namespace Consumer2;

public class Consumer2Worker(
    IEventConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<Consumer2Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Consumer2] Uruchomiono Consumer2Worker. Metoda: ExecuteAsync");
        logger.LogInformation("[Consumer2] Subskrypcja na kolejkę Typ2Event");

        consumer.Subscribe<Typ2Event>(@event =>
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Consumer2DbContext>();

            logger.LogInformation(
                "[Consumer2] Przetwarzanie Typ2Event (Id={EventId}, Source={Source}, Data={Data})",
                @event.Id, @event.SourceService, @event.Data);

            dbContext.ReceivedEvents.Add(new ReceivedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ2Event),
                Data = @event.Data,
                SourceService = @event.SourceService,
                ReceivedAt = DateTime.UtcNow
            });

            dbContext.SaveChanges();
            logger.LogInformation("[Consumer2] Zapisano odebrane zdarzenie do bazy danych");
        });

        return Task.CompletedTask;
    }
}
