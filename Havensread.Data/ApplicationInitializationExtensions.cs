using Havensread.Data.App;
using Havensread.Data.Ingestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;

namespace Havensread.Data;

public static class ApplicationInitializationExtensions
{
    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        return builder
            .AddDatabase<AppDbContext>("havensread-appdb", "app")
            .AddDatabase<IngestionDbContext>("havensread-ingestiondb", "ingestion");
    }

    private static IHostApplicationBuilder AddDatabase<TContext>(
        this IHostApplicationBuilder builder,
        string orchestrationName,
        string migrationsHistoryTableName) where TContext : DbContext
    {
        builder.AddNpgsqlDbContext<TContext>(orchestrationName, null, options =>
        {
            options.UseNpgsql(o =>
            {
                o.EnableRetryOnFailure();
                o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, migrationsHistoryTableName);
            })
            .UseSnakeCaseNamingConvention();

            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return builder;
    }
}
