using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using StokService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StokService.Consumers
{
    public class OrderCreatedEventConsumer(StokDbContext _dbContext) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            // Idempotent tasarımını gerceklestirdik, bir terslik olur da tekrar eden event gelirse Inbox Table a eklenemeyecek
            if (!_dbContext.OrderInboxes.Any(x => x.IdempotentToken.Equals(context.Message.IdempotentToken)))
            {
                await _dbContext.OrderInboxes.AddAsync(new()
                {
                    IdempotentToken = context.Message.IdempotentToken,
                    Payload = JsonSerializer.Serialize(context.Message),
                    Processed = false
                });

                await _dbContext.SaveChangesAsync();

            }
                
            // Yine bu asamayi da OrderOutbox Table'daki gibi belirli periyotlarla
            // Inbox Table'i kontrol edecek sekilde tasarlanmalidir. Simdilik boyle kalsin :)


            var orderInboxes = await _dbContext.OrderInboxes
                .Where(x => !x.Processed).ToListAsync();

            foreach (var order in orderInboxes)
            {
                var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(order.Payload);

                await Console.Out.WriteLineAsync($"{@event?.OrderId} numaralı siparişin stok işlemleri tamamlandı");

                order.Processed = true;
                await _dbContext.SaveChangesAsync();

            }


        }
    }
}
