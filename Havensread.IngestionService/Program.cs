using Havensread.Data;
using Havensread.IngestionService;
using Havensread.IngestionService.Workers;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Havensread.Connector;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabase();

builder.AddVectorStore();

builder.AddAIServices();

builder.AddHttpClients();

builder.Services.AddIngestionWorkers();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(nameof(IngestionWorker)));


// Add SignalR
builder.Services.AddSignalR();

// Configure MassTransit with RabbitMQ using Aspire's configuration
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<ProcessingCommandConsumer>();

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        // Uses configuration from Aspire
//        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));

//        cfg.ReceiveEndpoint("processing-queue", e =>
//        {
//            e.ConfigureConsumer<ProcessingCommandConsumer>(context);
//        });
//    });
//});

var app = builder.Build();
app.MapHub<JobProgressHub>("/jobprogresshub");
app.MapHub<WorkerHub>("/workerHub");
app.Run();
