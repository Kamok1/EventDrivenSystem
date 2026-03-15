using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Consumer4.Data;
using Microsoft.Extensions.Logging;

namespace Consumer4;

public class Consumer4Worker(
    IEventConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<Consumer4Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[Consumer4] Uruchomiono Consumer4Worker. Metoda: ExecuteAsync");
        logger.LogInformation("[Consumer4] Subskrypcja na kolejkę Typ4Event");

        consumer.Subscribe<Typ4Event>(@event =>
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Consumer4DbContext>();

            logger.LogInformation(
                "[Consumer4] Odebrano Typ4Event (Id={EventId}, Source={Source}, Data={Data})",
                @event.Id, @event.SourceService, @event.Data);

            dbContext.ReceivedEvents.Add(new ReceivedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ4Event),
                Data = @event.Data,
                SourceService = @event.SourceService,
                ReceivedAt = DateTime.UtcNow
            });

            dbContext.SaveChanges();
            logger.LogInformation("[Consumer4] Zapisano odebrane zdarzenie do bazy danych");
        });

        return Task.CompletedTask;
    }
}
