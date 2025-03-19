using System.Text;

namespace DataGenerator;

public static class InspirationHelper
{
    public static IEnumerable<string> ExtractConcepts(string text)
    {
        var uniqueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedText = text.Replace(" ", string.Empty);

        foreach (var key in Inspiration.Concepts.Keys)
        {
            var normalizedKey = key.Replace(" ", string.Empty);
            if (normalizedText.IndexOf(normalizedKey, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                uniqueKeys.Add(key);
            }
        }

        return uniqueKeys;
    }

    public static string GenerateRandomTagsText(IEnumerable<string> chosenConcepts)
    {
        StringBuilder sb = new();
        var concepts = chosenConcepts.ToArray();
        for (int i = 0; i < concepts.Length; i++)
        {
            var concept = concepts[i];
            if (!Inspiration.Concepts.TryGetValue(concept, out var options))
            {
                Console.WriteLine($"Couldn't find hallucinated concept {concept}");
                break;
            }
            var opts = options.ToArray();
            var index = Random.Shared.Next(0, opts.Length);
            var option = opts.ElementAt(index);
            sb.AppendLine($"Tag {i + 1} ({concept}): {option}.");
        }
        return sb.ToString();
    }
}