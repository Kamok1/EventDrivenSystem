using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Consumer2.Data;
using Consumer2;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Consumer2DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Consumer2Db")));

builder.Services.AddHostedService<Consumer2Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Consumer2DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
