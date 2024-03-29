using MassTransit;
using OrderOutboxTablePublisher;
using OrderOutboxTablePublisher.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

OrderOutboxSingeltonDatabase.InitilazeDb(builder.Configuration.GetConnectionString("MSSQL"));


builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddQuartz(configureator =>
{

    JobKey jobkey = new JobKey(nameof(OrderOutboxPublishJob));

    configureator.AddJob<OrderOutboxPublishJob>(options =>
    {
        options.WithIdentity(jobkey);
    });


    TriggerKey triggerKey = new TriggerKey("OrderOutboxPublishTrigger");
    configureator.AddTrigger(options =>
    {
        options.ForJob(jobkey)
        .WithIdentity(triggerKey)
        .StartAt(DateTime.UtcNow)
        .WithSimpleSchedule(builder =>
        {
            builder.WithIntervalInSeconds(5);
            builder.RepeatForever();
        });
    });


});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
