using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using StokService;
using StokService.Consumers;
using StokService.Data;
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<StokDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);

        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEvent, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});

var host = builder.Build();
host.Run();
