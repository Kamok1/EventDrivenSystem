using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Publisher2.Data;
using Publisher2;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Publisher2DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Publisher2Db")));

builder.Services.AddHostedService<Publisher2Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Publisher2DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
