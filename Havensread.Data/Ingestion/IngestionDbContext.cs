using Microsoft.EntityFrameworkCore;

namespace Havensread.Data.Ingestion;

public sealed class IngestionDbContext : DbContext
{
    public IngestionDbContext(DbContextOptions options) : base(options)
    { }

    public DbSet<IngestedDocument> Documents => Set<IngestedDocument>();
    public DbSet<IngestedRecord> Records => Set<IngestedRecord>();
    //public DbSet<SourceLink> SourceLinks => Set<SourceLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ingestion");

        modelBuilder.Entity<IngestedDocument>(document =>
        {
            document.HasKey(d => new { d.Id, d.Source });
            document.Property(d => d.Timestamp).HasColumnType("timestamp with time zone");
            document.HasMany(d => d.Records).WithOne().HasForeignKey(r => new { r.DocumentId, r.DocumentSource });
        });
    }
}
