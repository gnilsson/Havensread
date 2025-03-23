using Havensread.Data;
using Havensread.MigrationService.Workers;
using Havensread.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddVectorStore();

builder.Services.AddSingleton<AppWorker>();
builder.Services.AddSingleton<IngestionWorker>();
builder.Services.AddHostedService<WorkerCoordinator>();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(AppWorker.ActivitySourceName))
    .WithTracing(tracing => tracing.AddSource(IngestionWorker.ActivitySourceName));

builder.AddDatabase();

var host = builder.Build();
host.Run();
