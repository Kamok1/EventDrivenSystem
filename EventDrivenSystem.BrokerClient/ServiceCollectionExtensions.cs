using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventDrivenSystem.BrokerClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqBroker(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new RabbitMqSettings();
        configuration.GetSection("RabbitMq").Bind(settings);

        services.AddSingleton(settings);

        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqBrokerClient>>();
            return new RabbitMqBrokerClient(settings, logger);
        });

        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<RabbitMqBrokerClient>());
        services.AddSingleton<IEventConsumer>(sp => sp.GetRequiredService<RabbitMqBrokerClient>());

        return services;
    }
}
