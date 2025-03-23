using Havensread.ServiceDefaults;
using Microsoft.Extensions.AI;

namespace DataGenerator;

public sealed class BookIdea
{
    public required int Id { get; init; }
    public required int OutlineId { get; init; }
    public required ProcessResult ProcessResult { get; init; }
}


public sealed class ProcessResult
{
    public required Dictionary<int, ProcessIteration> History { get; init; }
}


public sealed class ProcessIteration
{
    public required string Idea { get; init; }
    public required string Phase { get; init; }
    public required string Revision { get; init; }
    public required string Summary { get; init; }
}


// first draft
// revising agent encouragements
// second draft

public enum Decision
{
    Complete,
    Revise,
    Continue,
}

public sealed class BookIdeaGenerator : GeneratorBase<BookIdea>
{
    private readonly IEnumerable<NarrativeOutline> _outlines;

    public BookIdeaGenerator(IServiceProvider services, IEnumerable<NarrativeOutline> outlines) : base(services)
    {
        _outlines = outlines;
    }

    protected override string DirectoryName => "bookIdeas";

    protected override object GetId(BookIdea item) => item.Id;
    protected override async IAsyncEnumerable<BookIdea> GenerateCoreAsync()
    {
        if (Directory.GetFiles(OutputDirPath).Length > 0) yield break;

        //C:\dev\Havensread\DataGenerator\prompts\bookIdeaPrompt.txt
        var solutionDir = PathUtils.SolutionDirectory;
        var promptsDir = Path.Combine(solutionDir, "DataGenerator", "prompts");



        //string[] concepts = [nameof(Inspiration.Settings), nameof(Inspiration.Themes), nameof(Inspiration.CharacterArchetypes)];

        var outlines = _outlines.ToArray();
        for (int i = 0; i < outlines.Length; i++)
        {
            var outline = outlines[i];
            var result = await ProcessAsync(promptsDir, outline);
            yield return new BookIdea
            {
                Id = i,
                OutlineId = outline.Id,
                ProcessResult = result
            };
        }




        Console.WriteLine("Stopping");
        await Task.Delay(1);
    }

    private string[] _phases = ["Narrative Outline", "Beginning", "Continuation", "Revision", "End"];
    private Dictionary<string, string> _phasePrompts = new()
    {
        ["Narrative Outline"] = "Narrative Outline: Write a brief narrative outline of the story.",
        ["Beginning"] = "Beginning: Write the beginning of the story.",
        ["Continuation"] = "Continuation: Continue the story or chapter.",
        ["Revision"] = "Revision: Revise the story or chapter.",
        ["End"] = "End: Write the end of the story.",
    };

    private async Task<ProcessResult> ProcessAsync(string promptsDir, NarrativeOutline outline)
    {
        var history = new Dictionary<int, ProcessIteration>();

        var tagsText = outline.TagsText;
        var phase = _phases[1];
        var recently = "[none]";
        var previously = "[none]";
        var coauthorNotes = "[none]";
        var iteration = -1;
        bool continueProcess = true;
        while (continueProcess)
        {
            iteration++;

            var prompt = await File.ReadAllTextAsync(Path.Combine(promptsDir, "ideaPrompt.txt"));
            var ideaPrompt = prompt
                .Replace("${{previously}}", previously)
                .Replace("${{phase}}", _phasePrompts[phase])
                .Replace("${{outline}}", outline.Text)
                .Replace("${{tags}}", tagsText)
                .Replace("${{recently}}", recently)
                .Replace("${{coauthor}}", coauthorNotes);

            var idea = await ChatClient.GetResponseAsync(
                ideaPrompt,
                new ChatOptions { Temperature = 0.9f, StopSequences = ["END_OF_CONTENT"] });

            if (phase is "End" || iteration == 10)
            {
                continueProcess = false;
                break;
            }

            var revisionPrompt = await File.ReadAllTextAsync(Path.Combine(promptsDir, "revisionPrompt.txt"));
            var revision = await ChatClient.GetResponseAsync(
                revisionPrompt.Replace("${{story}}", idea.Text),
                new ChatOptions { Temperature = 0.7f, StopSequences = ["END_OF_CONTENT"] });
            coauthorNotes = revision.Text;

            var summaryPrompt = await File.ReadAllTextAsync(Path.Combine(promptsDir, "summaryPrompt.txt"));
            var summary = await ChatClient.GetResponseAsync(
                summaryPrompt.Replace("${{story}}", idea.Text),
                new ChatOptions { Temperature = 0.5f, StopSequences = ["END_OF_CONTENT"] });

            var concepts = InspirationHelper.ExtractConcepts(revision.Text);
            tagsText = InspirationHelper.GenerateRandomTagsText(concepts);

            var recentlyTextToRemove = iteration == 0 ? "[none]" : history[iteration - 1].Idea;
            recently = recently.Replace(recentlyTextToRemove, idea.Text);
            previously = iteration == 0
                ? previously.Replace("[none]", summary.Text)
                : $"{previously}{Environment.NewLine}{summary.Text}";

            var historyText = string.Join(Environment.NewLine, history.Values.Select(h => h.Idea));
            var decisionPrompt = await File.ReadAllTextAsync(Path.Combine(promptsDir, "decisionPrompt.txt"));
            var decisionResponse = await ChatClient.GetResponseAsync(
                decisionPrompt
                .Replace("${{outline}}", outline.Text)
                .Replace("${{story}}", $"{historyText}{Environment.NewLine}{idea.Text}"),
                new ChatOptions { Temperature = 0.3f, StopSequences = ["END_OF_CONTENT"] });

            if (phase != _phases[4])
            {
                if (Enum.TryParse<Decision>(decisionResponse.Text, true, out var decision))
                {
                    phase = decision switch
                    {
                        Decision.Complete => _phases[4],
                        Decision.Revise => _phases[3],
                        Decision.Continue => _phases[2],
                        _ => null!
                    };
                }
                else
                {
                    Console.WriteLine("Ambiguous decision.");
                    break;
                }
            }

            history[iteration] = new ProcessIteration()
            {
                Idea = idea.Text,
                Revision = revision.Text,
                Phase = phase,
                Summary = summary.Text,
            };
        }

        return new ProcessResult { History = history };
    }
}
