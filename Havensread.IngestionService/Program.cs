using Havensread.Data;
using Havensread.IngestionService;
using Havensread.IngestionService.Workers;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Havensread.Connector;
using Havensread.Connector.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabase();

builder.AddVectorStore();

builder.AddAIServices();

builder.AddHttpClients();

builder.Services.AddWorkers();

builder.Services.AddSignalR();

//builder.AddRabbitMQClient(connectionName: "havensread-rabbitmq");

//builder.Services.AddMassTransit(x =>
//{
//    x.SetKebabCaseEndpointNameFormatter();
//    x.AddConsumer<WorkerCommandConsumer>();

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        var configuration = context.GetRequiredService<IConfiguration>();
//        var connection = configuration.GetConnectionString("havensread-rabbitmq");
//        cfg.Host(connection);
//        cfg.ConfigureEndpoints(context);
//    });
//});

var app = builder.Build();
//app.MapHub<JobProgressHub>("/jobprogresshub");
app.MapHub<WorkerHub>("/workerHub");
app.Run();
