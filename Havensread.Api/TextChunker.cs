//using System.Text.RegularExpressions;

//namespace Havensread.Api;

//public class TextChunker
//{
//    // Basic paragraph-based chunking
//    public static IEnumerable<string> ChunkByParagraph(string text, int maxTokens, int overlapTokens = 0)
//    {
//        var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
//        var chunks = new List<string>();
//        var currentChunk = new List<string>();
//        int currentTokens = 0;

//        foreach (var paragraph in paragraphs)
//        {
//            int paraTokens = EstimateTokenCount(paragraph);

//            if (currentTokens + paraTokens > maxTokens && currentChunk.Any())
//            {
//                chunks.Add(string.Join("\n\n", currentChunk));
//                currentChunk.Clear();
//                currentTokens = 0;
//            }

//            if (paraTokens > maxTokens)
//            {
//                foreach (var subChunk in SplitLargeText(paragraph, maxTokens))
//                {
//                    chunks.Add(subChunk);
//                }
//            }
//            else
//            {
//                currentChunk.Add(paragraph);
//                currentTokens += paraTokens;
//            }
//        }

//        if (currentChunk.Any()) chunks.Add(string.Join("\n\n", currentChunk));
//        return ApplyOverlap(chunks, overlapTokens);
//    }

//    // Section-based chunking (Markdown compatible)
//    public static IEnumerable<string> ChunkBySection(string text, int maxTokens)
//    {
//        var sections = new List<string>();
//        var lines = text.Split('\n');
//        var currentSection = new List<string>();

//        foreach (var line in lines)
//        {
//            if (IsHeading(line) && currentSection.Any())
//            {
//                sections.Add(string.Join("\n", currentSection));
//                currentSection.Clear();
//            }
//            currentSection.Add(line);
//        }
//        sections.Add(string.Join("\n", currentSection));

//        return ProcessSections(sections, maxTokens);
//    }

//    // Sliding window chunking
//    public static IEnumerable<string> SlidingWindowChunk(string text, int windowSize, int overlapSize)
//    {
//        var tokens = Tokenize(text).ToList();
//        int start = 0;

//        while (start < tokens.Count)
//        {
//            int end = Math.Min(start + windowSize, tokens.Count);
//            yield return string.Join(" ", tokens.Skip(start).Take(end - start));
//            start += (windowSize - overlapSize);
//        }
//    }

//    private static IEnumerable<string> ProcessSections(List<string> sections, int maxTokens)
//    {
//        foreach (var section in sections)
//        {
//            if (EstimateTokenCount(section) <= maxTokens)
//            {
//                yield return section;
//            }
//            else
//            {
//                foreach (var chunk in ChunkByParagraph(section, maxTokens))
//                {
//                    yield return chunk;
//                }
//            }
//        }
//    }

//    private static IEnumerable<string> SplitLargeText(string text, int maxTokens)
//    {
//        int current = 0;
//        while (current < text.Length)
//        {
//            int chunkSize = Math.Min(maxTokens * 4, text.Length - current);
//            yield return text.Substring(current, chunkSize);
//            current += chunkSize;
//        }
//    }

//    private static IEnumerable<string> ApplyOverlap(List<string> chunks, int overlapTokens)
//    {
//        if (overlapTokens <= 0 || chunks.Count < 2) return chunks;

//        var overlapped = new List<string>();
//        for (int i = 0; i < chunks.Count; i++)
//        {
//            if (i > 0)
//            {
//                int overlapSize = Math.Min(overlapTokens * 4, chunks[i - 1].Length);
//                string overlap = chunks[i - 1].Substring(chunks[i - 1].Length - overlapSize);
//                overlapped.Add(overlap + chunks[i]);
//            }
//            else
//            {
//                overlapped.Add(chunks[i]);
//            }
//        }
//        return overlapped;
//    }

//    private static bool IsHeading(string line)
//    {
//        return Regex.IsMatch(line, @"^#+\s+") ||
//               Regex.IsMatch(line, @"^=+$|^-+$");
//    }

//    private static int EstimateTokenCount(string text)
//    {
//        // Simple estimation: 1 token ≈ 4 characters
//        return (int)Math.Ceiling(text.Length / 4.0);
//    }

//    private static IEnumerable<string> Tokenize(string text)
//    {
//        // Simple whitespace tokenizer (replace with actual tokenizer)
//        return text.Split(new[] { ' ', '\n', '\t', '\r' },
//                       StringSplitOptions.RemoveEmptyEntries);
//    }
//}
