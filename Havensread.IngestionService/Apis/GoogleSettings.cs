namespace Havensread.IngestionService.Apis;

public sealed class GoogleSettings
{
    public const string SectionName = "GoogleSearch";
    public required string Token { get; init; }
    public required string SearchEngineId { get; init; }
}
