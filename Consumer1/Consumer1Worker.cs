using EventDrivenSystem.BrokerClient;
using EventDrivenSystem.Models;
using Consumer1.Data;
using Microsoft.Extensions.Logging;

namespace Consumer1;

public class Consumer1Worker(
    IEventConsumer consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<Consumer1Worker> logger,
    string instanceName) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[{Instance}] Uruchomiono Consumer1Worker. Metoda: ExecuteAsync", instanceName);
        logger.LogInformation("[{Instance}] Subskrypcja na kolejkę Typ1Event", instanceName);

        consumer.Subscribe<Typ1Event>(@event =>
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Consumer1DbContext>();

            logger.LogInformation(
                "[{Instance}] Przetwarzanie Typ1Event (Id={EventId}, Source={Source}, Data={Data})",
                instanceName, @event.Id, @event.SourceService, @event.Data);

            dbContext.ReceivedEvents.Add(new ReceivedEventLog
            {
                EventId = @event.Id,
                EventType = nameof(Typ1Event),
                Data = @event.Data,
                SourceService = @event.SourceService,
                ReceivedAt = DateTime.UtcNow
            });

            dbContext.SaveChanges();
            logger.LogInformation("[{Instance}] Zapisano odebrane zdarzenie do bazy danych", instanceName);
        });

        return Task.CompletedTask;
    }
}
