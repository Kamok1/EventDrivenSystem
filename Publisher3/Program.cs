using Microsoft.EntityFrameworkCore;
using EventDrivenSystem.BrokerClient;
using Publisher3.Data;
using Publisher3;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqBroker(builder.Configuration);

builder.Services.AddDbContext<Publisher3DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Publisher3Db")));

builder.Services.AddHostedService<Publisher3Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Publisher3DbContext>();
    await db.Database.EnsureCreatedAsync();
}

host.Run();
