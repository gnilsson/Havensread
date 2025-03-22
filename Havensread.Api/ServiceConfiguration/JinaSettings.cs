namespace Havensread.Api.ServiceConfiguration;

public sealed class JinaSettings
{
    public const string SectionName = "JinaAI";

    public required string Token { get; init; }
}
