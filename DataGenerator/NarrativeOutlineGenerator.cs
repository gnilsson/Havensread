using Havensread.ServiceDefaults;
using Microsoft.Extensions.AI;

namespace DataGenerator;

public sealed class NarrativeOutline
{
    public required int Id { get; init; }
    public required string Text { get; init; }
    public required string TagsText { get; init; }
}

public sealed class NarrativeOutlineGenerator : GeneratorBase<NarrativeOutline>
{
    public NarrativeOutlineGenerator(IServiceProvider service) : base(service)
    { }

    protected override string DirectoryName => "narrativeOutlines";

    protected override object GetId(NarrativeOutline item) => item.Id;

    protected override async IAsyncEnumerable<NarrativeOutline> GenerateCoreAsync()
    {
        if (Directory.GetFiles(OutputDirPath).Length > 0) yield break;

        var promptPath = Path.Combine(PathUtils.SolutionDirectory, "DataGenerator", "prompts", "narrativeOutlinePrompt.txt");

        for (int i = 0; i < 5; i++)
        {
            yield return await ProcessAsync(i, promptPath);
        }
    }

    private async Task<NarrativeOutline> ProcessAsync(int iteration, string promptPath)
    {
        var keys = Inspiration.Concepts.Keys.ToArray();
        var randomConcepts = Enumerable.Range(0, 3).Select(_ => keys[Random.Shared.Next(0, keys.Length)]).ToArray();
        var tagsText = InspirationHelper.GenerateRandomTagsText(randomConcepts);
        var prompt = await File.ReadAllTextAsync(promptPath);

        var response = await ChatClient.GetResponseAsync(
            prompt.Replace("${{tags}}", tagsText),
            new ChatOptions { Temperature = 0.9f, StopSequences = ["END_OF_CONTENT"] });

        return new NarrativeOutline
        {
            Id = iteration,
            Text = response.Text,
            TagsText = tagsText,
        };
    }
}
