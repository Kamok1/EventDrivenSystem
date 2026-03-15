using EventDrivenSystem.Models;

namespace EventDrivenSystem.BrokerClient;

public interface IEventPublisher : IDisposable
{
    void Publish<TEvent>(TEvent @event) where TEvent : BaseEvent;
}
