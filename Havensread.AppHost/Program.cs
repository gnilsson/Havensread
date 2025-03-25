var builder = DistributedApplication.CreateBuilder(args);

var dbPassword = builder.AddParameter("PostgresPassword", secret: true);

var pgsql = builder
    .AddPostgres("havensread-sql-postgres", password: dbPassword)
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var appDb = pgsql.AddDatabase("havensread-appdb");
var ingDb = pgsql.AddDatabase("havensread-ingestiondb");

var vectorDb = builder
    .AddQdrant("havensread-vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder
    .AddRabbitMQ("havensread-rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var migrations = builder.AddProject<Projects.Havensread_MigrationService>("migrations")
    .WithReference(appDb)
    .WaitFor(appDb)
    .WithReference(ingDb)
    .WaitFor(ingDb)
    .WithReference(vectorDb)
    .WaitFor(vectorDb);

var api = builder
    .AddProject<Projects.Havensread_Api>("havensread-api")
    .WithReference(appDb)
    .WaitFor(appDb)
    .WithReference(ingDb)
    .WaitFor(ingDb)
    .WaitFor(migrations)
    .WithReference(vectorDb);

var ingestion = builder
    .AddProject<Projects.Havensread_IngestionService>("ingestion")
    .WithReference(appDb)
    .WaitFor(appDb)
    .WithReference(ingDb)
    .WaitFor(ingDb)
    .WithReference(vectorDb)
    .WaitFor(vectorDb)
    .WithReference(rabbitmq);

var web = builder
    .AddProject<Projects.Havensread_Web>("havensread-web")
    .WithReference(appDb)
    .WaitFor(appDb)
    .WithReference(api)
    .WaitFor(migrations)
    .WithReference(vectorDb)
    .WithReference(rabbitmq);

builder.Build().Run();
