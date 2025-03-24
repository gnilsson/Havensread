using Havensread.Data;
using Havensread.MigrationService.Workers;
using Havensread.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddVectorStore();

builder.Services.AddSingleton<AppMigrationWorker>();
builder.Services.AddSingleton<IngestionMigrationWorker>();
builder.Services.AddHostedService<MigrationWorkerCoordinator>();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(AppMigrationWorker.ActivitySourceName))
    .WithTracing(tracing => tracing.AddSource(IngestionMigrationWorker.ActivitySourceName));

builder.AddDatabase();

var host = builder.Build();
host.Run();
