using Havensread.Connector;
using Havensread.Data;
using Havensread.IngestionService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddDatabase();

builder.AddVectorStore();

builder.AddAIServices();

builder.AddHttpClients();

builder.Services.AddWorkers();

builder.Services.AddSignalR(o =>
{
    if (builder.Environment.IsDevelopment())
    {
        o.EnableDetailedErrors = true;
    }
});

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
