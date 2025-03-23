namespace Havensread.IngestionService.Apis;

public sealed class JinaSettings
{
    public const string SectionName = "JinaAI";

    public required string Token { get; init; }
}
