using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Publisher1.Data;
using Publisher1;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Publisher1DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Publisher1Db")));

builder.Services.AddSingleton<IHostedService>(sp =>
    new Publisher1Worker(
        sp.GetRequiredService<IEventPublisher>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<Publisher1Worker>>(),
        "Publisher1-A"));

builder.Services.AddSingleton<IHostedService>(sp =>
    new Publisher1Worker(
        sp.GetRequiredService<IEventPublisher>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<Publisher1Worker>>(),
        "Publisher1-B"));

builder.Services.AddSingleton<IHostedService>(sp =>
    new Publisher1Worker(
        sp.GetRequiredService<IEventPublisher>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<ILogger<Publisher1Worker>>(),
        "Publisher1-C"));

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Publisher1DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
