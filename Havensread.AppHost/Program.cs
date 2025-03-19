var builder = DistributedApplication.CreateBuilder(args);

var dbPassword = builder.AddParameter("PostgresPassword", secret: true);

var pgsql = builder
    .AddPostgres("havensread-sql-postgres", password: dbPassword)
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("havensread-pgsql");

var api = builder
    .AddProject<Projects.Havensread_Api>("havensread-api")
    .WithReference(pgsql)
    .WaitFor(pgsql);

builder.AddProject<Projects.Havensread_MigrationService>("migrations")
    .WithReference(pgsql)
    .WaitFor(pgsql);

var web = builder
    .AddProject<Projects.Havensread_Web>("havensread-web")
    .WithReference(pgsql)
    .WaitFor(pgsql)
    .WithReference(api);

builder.Build().Run();
