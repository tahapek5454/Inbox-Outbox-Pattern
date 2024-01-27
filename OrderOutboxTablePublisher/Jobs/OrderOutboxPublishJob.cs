using MassTransit;
using OrderOutboxTablePublisher.Models;
using Quartz;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderOutboxTablePublisher.Jobs
{
    public class OrderOutboxPublishJob(IPublishEndpoint _publishEndpoint) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderOutboxSingeltonDatabase.DataReaderState)
            {
                OrderOutboxSingeltonDatabase.DataReaderBusy();

                List<OrderOutbox> orderOutboxes =  (await OrderOutboxSingeltonDatabase
                    .QueryAsync<OrderOutbox>
                    ($@"SELECT * FROM OrderOutboxes oo WHERE oo.ProcessedDate  IS NULL  ORDER BY oo.OccuredOn ASC"))
                    .ToList();

                foreach(var orderOutbox in orderOutboxes)
                {
                    if (orderOutbox.Type != nameof(OrderCreatedEvent))
                        continue;


                    OrderCreatedEvent? orderCreadtedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>
                        (orderOutbox.Payload);

                    if(orderCreadtedEvent is not null)
                    {
                        var query = $@"UPDATE  OrderOutboxes SET ProcessedDate = GETDATE() WHERE IdempotentToken = '{orderOutbox.IdempotentToken}'";
                        await _publishEndpoint.Publish(orderCreadtedEvent);
                        _ = (await OrderOutboxSingeltonDatabase
                                .ExecuteAsync
                                (query));

                    }

                }

                OrderOutboxSingeltonDatabase.DataReaderReady();

                await Console.Out.WriteAsync("Order Outbox Table Check");
            }
        }
    }
}
