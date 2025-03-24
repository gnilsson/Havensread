namespace Havensread.Data.Ingestion;

//public sealed class ExternalSource
//{
//    public int Id { get; init; }

//    public required string Name { get; init; }

//    public required SourceReadingStrategy ReadingStrategy { get; init; }

//    public ICollection<SourceLink> Links { get; init; } = [];


//    // document id?
//}

//[Flags]
//public enum SourceReadingStrategy
//{
//    Unknown = 0,
//}


public sealed class SourceLink
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string Url { get; init; }
}
