using System.Reflection;
using Microsoft.Extensions.Logging;
using EventDrivenSystem.Models;

namespace EventDrivenSystem.BrokerClient;

public static class EventScanner
{
    public static IReadOnlyList<Type> DiscoverEventTypes(ILogger logger)
    {
        var assembly = Assembly.GetAssembly(typeof(BaseEvent))
            ?? throw new InvalidOperationException("Nie znaleziono assembly EventDrivenSystem.Models");

        logger.LogInformation("[Reflection] Skanowanie assembly: {AssemblyName}", assembly.GetName().Name);

        var eventTypes = assembly.GetTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && t.Name.EndsWith("Event", StringComparison.Ordinal)
                        && t.IsSubclassOf(typeof(BaseEvent)))
            .OrderBy(t => t.Name)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            logger.LogInformation("[Reflection] Wykryto typ zdarzenia: {EventType}", eventType.Name);
        }

        logger.LogInformation("[Reflection] Łącznie wykryto {Count} typów zdarzeń", eventTypes.Count);

        return eventTypes;
    }
    
    public static string GetQueueName(Type eventType) => eventType.Name;
    public static string GetQueueName<TEvent>() where TEvent : BaseEvent => typeof(TEvent).Name;
}
