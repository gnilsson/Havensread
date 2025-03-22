using Havensread.Data.App;
using Havensread.Data.Ingestion;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Havensread.MigrationService;

public sealed class Worker : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime)
    {
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);
        var slnDir = PathUtils.FindAncestorDirectoryContaining("*.sln");

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!await IsDatabaseUpToDateAsync(appDbContext, cancellationToken))
            {
                await RunMigrationAsync(appDbContext, cancellationToken);
                await WriteDiagramModelAsync(appDbContext, slnDir);
            }

            if (!await appDbContext.Books.AnyAsync())
            {
                await SeedDataAsync(appDbContext, slnDir, cancellationToken);
            }

            var ingestionDbContext = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();

            if (!await IsDatabaseUpToDateAsync(ingestionDbContext, cancellationToken))
            {
                await RunMigrationAsync(ingestionDbContext, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        _hostApplicationLifetime.StopApplication();
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

    private static async Task SeedDataAsync(DbContext dbContext, string slnDir, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await DatabaseSeedHelper.SeedDataAsync(dbContext, slnDir, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task WriteDiagramModelAsync(DbContext dbContext, string slnDir)
    {
        await File.WriteAllTextAsync(
            Path.Combine(slnDir, "Havensread.Data", "appdb.dgml"),
            dbContext.AsDgml());
    }
}
