using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Consumer4.Data;
using Consumer4;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Consumer4DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Consumer4Db")));

builder.Services.AddHostedService<Consumer4Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Consumer4DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
