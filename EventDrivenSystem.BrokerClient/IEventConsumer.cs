using EventDrivenSystem.Models;

namespace EventDrivenSystem.BrokerClient;

public interface IEventConsumer : IDisposable
{
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent;
}
