using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EventDrivenSystem.Models;

namespace EventDrivenSystem.BrokerClient;

public class RabbitMqBrokerClient : IEventPublisher, IEventConsumer
{
    private readonly ILogger _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqBrokerClient(RabbitMqSettings settings, ILogger logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory();

        if (!string.IsNullOrWhiteSpace(settings.Url))
        {
            factory.Uri = new Uri(settings.Url);
            _logger.LogInformation("[BrokerClient] Łączenie z RabbitMQ przez URL");
        }
        else
        {
            factory.HostName = settings.HostName;
            factory.Port = settings.Port;
            factory.UserName = settings.UserName;
            factory.Password = settings.Password;
            factory.VirtualHost = settings.VirtualHost;

            if (settings.UseSsl)
            {
                factory.Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = settings.HostName
                };
                _logger.LogInformation("[BrokerClient] SSL/TLS włączone dla połączenia");
            }

            _logger.LogInformation("[BrokerClient] Łączenie z RabbitMQ: {Host}:{Port}", settings.HostName, settings.Port);
        }

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _logger.LogInformation("[BrokerClient] Połączono z RabbitMQ pomyślnie");

        var eventTypes = EventScanner.DiscoverEventTypes(_logger);
        foreach (var eventType in eventTypes)
        {
            EnsureQueueExists(EventScanner.GetQueueName(eventType));
        }
    }
    private void EnsureQueueExists(string queueName)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("[BrokerClient] QueueDeclare — kolejka '{QueueName}' jest gotowa", queueName);
    }
    
    public void Publish<TEvent>(TEvent @event) where TEvent : BaseEvent
    {
        var queueName = EventScanner.GetQueueName<TEvent>();
        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "[Publisher] Wysłano {EventType} (Id={EventId}) na kolejkę '{Queue}'. Data: {Data}",
            typeof(TEvent).Name, @event.Id, queueName, @event.Data);
    }
    
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent
    {
        var queueName = EventScanner.GetQueueName<TEvent>();

        EnsureQueueExists(queueName);

        _logger.LogInformation("[Consumer] Rozpoczęto nasłuchiwanie na kolejce '{Queue}'", queueName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var @event = JsonSerializer.Deserialize<TEvent>(json);

                if (@event is not null)
                {
                    _logger.LogInformation(
                        "[Consumer] Odebrano {EventType} (Id={EventId}) z kolejki '{Queue}'",
                        typeof(TEvent).Name, @event.Id, queueName);

                    handler(@event);
                }

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Consumer] Błąd przetwarzania wiadomości z kolejki '{Queue}'", queueName);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _logger.LogInformation("[BrokerClient] Połączenie z RabbitMQ zamknięte");
    }
}
