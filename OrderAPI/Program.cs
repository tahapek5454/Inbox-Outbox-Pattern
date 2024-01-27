using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Data.Contexts;
using OrderAPI.Models;
using OrderAPI.ViewModels;
using Shared;
using Shared.Events;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderDbContext orderDbContext, ISendEndpointProvider sendEndpointProvider) =>
{
    Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId,
        }).ToList(),
    };

    await orderDbContext.Orders.AddAsync(order);


    var idempotentToken = Guid.NewGuid();
    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Shared.Datas.OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId
        }).ToList(),
        IdempotentToken = idempotentToken
    };


    OrderOutbox orderOutbox = new()
    {
        OccuredOn = DateTime.UtcNow,
        ProcessedDate = null,
        Payload = JsonSerializer.Serialize(orderCreatedEvent),
        Type = orderCreatedEvent.GetType().Name,
        IdempotentToken = idempotentToken
    };

    await orderDbContext.OrderOutboxes.AddAsync(orderOutbox);

    await orderDbContext.SaveChangesAsync();

    // Artik direkt messageBroker a biz gondermiyoruz. Outbox table a kaydediyoruz. Alttaki Eski yontem

    //var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEvent}"));
    //await sendEndpoint.Send<OrderCreatedEvent>(orderCreatedEvent);


});




app.Run();
