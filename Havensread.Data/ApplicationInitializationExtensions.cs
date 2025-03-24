using Havensread.Api.Data;
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
            .AddDatabaseContext<AppDbContext>("havensread-appdb", AppDbContext.SchemaName)
            .AddDatabaseContext<IngestionDbContext>("havensread-ingestiondb", IngestionDbContext.SchemaName, options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.AddInterceptors(new DevelopmentIngestionInterceptor());
                }
            });
    }

    private static IHostApplicationBuilder AddDatabaseContext<TContext>(
        this IHostApplicationBuilder builder,
        string orchestrationName,
        string migrationsHistoryTableName,
        Action<DbContextOptionsBuilder>? additionalOptions = null) where TContext : DbContext
    {
        builder.AddNpgsqlDbContext<TContext>(orchestrationName, null, options =>
        {
            additionalOptions?.Invoke(options);

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
