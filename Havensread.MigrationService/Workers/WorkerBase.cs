using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Havensread.MigrationService.Workers;

public abstract class WorkerBase<TContext> where TContext : DbContext
{
    private readonly ActivitySource _activitySource;
    private readonly IServiceProvider _serviceProvider;
    private readonly bool _forceSeed;

    protected WorkerBase(IServiceProvider serviceProvider, string activitySourceName, bool forceSeed = false)
    {
        _serviceProvider = serviceProvider;
        _forceSeed = forceSeed;
        _activitySource = new(activitySourceName);
    }

    protected abstract string ServiceName { get; }
    protected abstract string SchemaName { get; }
    protected virtual IEnumerable<string> ExcludedFromSeedingTableNames { get; } = [];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(ServiceName, ActivityKind.Client);

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

            if (!await IsDatabaseUpToDateAsync(dbContext, cancellationToken))
            {
                await RunMigrationAsync(dbContext, cancellationToken);
                await WriteDiagramModelAsync(dbContext, PathUtils.SolutionDirectory, SchemaName);
            }

            if (await ShouldSeedDataAsync(dbContext, cancellationToken) || _forceSeed) // incremental seed?
            {
                await SeedDataAsync(dbContext, PathUtils.SolutionDirectory, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }
    }

    protected abstract Task GenerateSeedDataAsync(TContext dbContext, string slnDir, CancellationToken cancellationToken);
    protected abstract Task<bool> ShouldSeedDataAsync(TContext dbContext, CancellationToken cancellationToken);

    // note:
    // what about the case when we want to force seed
    //private async Task<bool> HasAnyDataAsync(DbContext dbContext, CancellationToken cancellationToken)
    //{
    //    var tableNames = dbContext.Model
    //        .GetEntityTypes()
    //        .Select(t => t.GetTableName())
    //        .Where(t => !ExcludedFromSeedingTableNames.Contains(t));

    //    foreach (var tableName in tableNames)
    //    {
    //        var sql = $"SELECT 1 FROM {tableName} LIMIT 1";
    //        var result = await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    //        if (result > 0) return true;
    //    }

    //    return false;
    //}

    private async Task SeedDataAsync(TContext dbContext, string slnDir, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await GenerateSeedDataAsync(dbContext, slnDir, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task<bool> IsDatabaseUpToDateAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

        return !pendingMigrations.Except(appliedMigrations).Any();
    }

    private static async Task RunMigrationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task WriteDiagramModelAsync(DbContext dbContext, string slnDir, string schemaName)
    {
        await File.WriteAllTextAsync(
            Path.Combine(slnDir, "Havensread.Data", $"{schemaName}db.dgml"),
            dbContext.AsDgml());
    }
}
