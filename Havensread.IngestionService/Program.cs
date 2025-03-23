using Havensread.IngestionService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<WorkerCoordinator>();

var host = builder.Build();
host.Run();
