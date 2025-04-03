namespace Havensread.Data.Ingestion;

public sealed class IngestedRecord
{
    public required Guid Id { get; init; }

    public Guid DocumentId { get; init; }

    public string DocumentSource { get; init; } = default!;

    public required string SourceLink { get; init; }
}
