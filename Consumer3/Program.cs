using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Consumer3.Data;
using Consumer3;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Consumer3DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Consumer3Db")));

builder.Services.AddHostedService<Consumer3Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Consumer3DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
