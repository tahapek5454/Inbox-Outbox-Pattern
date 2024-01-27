using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Data.Contexts;
using OrderAPI.Models;
using OrderAPI.ViewModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderDbContext orderDbContext) =>
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
    await orderDbContext.SaveChangesAsync();
});




app.Run();
