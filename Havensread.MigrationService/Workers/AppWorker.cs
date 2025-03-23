using Havensread.Data.App;
using Microsoft.EntityFrameworkCore;

namespace Havensread.MigrationService.Workers;

public sealed class AppWorker : WorkerBase<AppDbContext>
{
    public const string ActivitySourceName = "App Migrations";

    public AppWorker(IServiceProvider serviceProvider) : base(serviceProvider, ActivitySourceName)
    { }

    protected override string ServiceName => "Migrating app context";
    protected override string SchemaName { get; } = AppDbContext.SchemaName;
    //protected override IEnumerable<string> ExcludedFromSeedingTableNames { get; } = ["books_authors", "authors_books"];

    protected override Task GenerateSeedDataAsync(AppDbContext dbContext, string slnDir, CancellationToken cancellationToken)
    {
        return DatabaseSeedHelper.GenerateAppSeedDataAsync(dbContext, slnDir, cancellationToken);
    }

    protected override async Task<bool> ShouldSeedDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        return !await dbContext.Books.AnyAsync(cancellationToken);
    }
}
