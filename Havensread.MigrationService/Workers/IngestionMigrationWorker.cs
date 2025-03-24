using Havensread.Data.Ingestion;
using Microsoft.EntityFrameworkCore;

namespace Havensread.MigrationService.Workers;

public sealed class IngestionMigrationWorker : MigrationWorkerBase<IngestionDbContext>
{
    public const string ActivitySourceName = "Ingestion Migrations";

    private readonly IServiceProvider _serviceProvider;

    public IngestionMigrationWorker(IServiceProvider serviceProvider) : base(serviceProvider, ActivitySourceName)
    {
        _serviceProvider = serviceProvider;
    }

    protected override string ServiceName => "Migrating ingestion context";
    protected override string SchemaName { get; } = IngestionDbContext.SchemaName;

    protected override Task GenerateSeedDataAsync(IngestionDbContext dbContext, string slnDir, CancellationToken cancellationToken)
    {
        return DatabaseSeedHelper.GenerateIngestionSeedDataAsync(_serviceProvider, dbContext, slnDir, cancellationToken);
    }

    protected override async Task<bool> ShouldSeedDataAsync(IngestionDbContext dbContext, CancellationToken cancellationToken)
    {
        return !await dbContext.Documents.AnyAsync(cancellationToken);
    }
}
