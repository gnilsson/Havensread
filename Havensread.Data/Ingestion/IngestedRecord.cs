namespace Havensread.Data.Ingestion;

public sealed class IngestedRecord
{
    public required string Id { get; init; }

    public Guid DocumentId { get; init; }

    public string DocumentSource { get; init; } = default!;
}
