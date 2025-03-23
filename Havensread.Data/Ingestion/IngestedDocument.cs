namespace Havensread.Data.Ingestion;

public sealed class IngestedDocument
{
    public required Guid Id { get; init; }

    public required string Source { get; init; }

    //public required SourceReadingStrategy ReadingStrategy { get; init; }

    public int Version { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    //public ICollection<SourceLink> Links { get; init; } = [];

    public ICollection<IngestedRecord> Records { get; init; } = [];
}
