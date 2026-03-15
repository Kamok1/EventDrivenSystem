using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Consumer1.Data;
using Consumer1;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Consumer1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Consumer1Db")));

builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer1Worker(
        sp.GetRequiredService<IEventConsumer>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<Consumer1Worker>>(),
        "Consumer1-A"));

builder.Services.AddSingleton<IHostedService>(sp =>
    new Consumer1Worker(
        sp.GetRequiredService<IEventConsumer>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<Consumer1Worker>>(),
        "Consumer1-B"));

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Consumer1DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
