//using Microsoft.SemanticKernel.Text;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace Havensread.Api;

//public sealed class DocumentAwareChunker
//{
//    public record ChunkWithContext(string Content, string Outline, string SectionPath);

//    private class OutlineNode
//    {
//        public int StartPosition { get; set; }
//        public int EndPosition { get; set; }
//        public string Title { get; }
//        public int Level { get; }
//        public List<OutlineNode> Children { get; } = new();

//        public OutlineNode(string title, int level, int start, int end)
//        {
//            Title = title;
//            Level = level;
//            StartPosition = start;
//            EndPosition = end;
//        }
//    }

//    public static List<ChunkWithContext> ChunkWithOutline(
//        string documentText,
//        int maxTokensPerChunk,
//        bool isMarkdown,
//        TextChunker.TokenCounter? tokenCounter = null)
//    {
//        var outline = isMarkdown ?
//            ExtractMarkdownOutline(documentText) :
//            ExtractPlainTextOutline(documentText);

//        var chunks = isMarkdown ?
//            TextChunker.SplitMarkdownParagraphs(new[] { documentText }, maxTokensPerChunk, 0, tokenCounter: tokenCounter) :
//            TextChunker.SplitPlainTextParagraphs(new[] { documentText }, maxTokensPerChunk, 0, tokenCounter: tokenCounter);

//        return EnhanceChunksWithOutline(documentText, chunks, outline, maxTokensPerChunk, tokenCounter);
//    }

//    private static List<ChunkWithContext> EnhanceChunksWithOutline(
//        string fullText,
//        List<string> chunks,
//        List<OutlineNode> outline,
//        int maxTokens,
//        TextChunker.TokenCounter? tokenCounter)
//    {
//        var result = new List<ChunkWithContext>();
//        var currentOutline = CompressOutline(outline, maxTokens / 4, tokenCounter);
//        int chunkIndex = 0;

//        foreach (var chunk in chunks)
//        {
//            var position = fullText.IndexOf(chunk, StringComparison.Ordinal);
//            var chunkLocation = LocateChunkInOutline(outline, position, chunk.Length);

//            var chunkHeader = new StringBuilder();
//            chunkHeader.AppendLine($"## Document Context (Chunk {++chunkIndex} of {chunks.Count})");
//            chunkHeader.AppendLine(currentOutline);
//            chunkHeader.AppendLine($"### Current Section: {chunkLocation.SectionPath}");
//            chunkHeader.AppendLine($"**Covers:** {chunkLocation.StartSection} to {chunkLocation.EndSection}");
//            chunkHeader.AppendLine("---");

//            int headerTokens = GetTokenCount(chunkHeader.ToString(), tokenCounter);
//            string chunkContent = TruncateToTokenCount(chunk, maxTokens - headerTokens, tokenCounter);

//            result.Add(new ChunkWithContext(
//                Content: chunkHeader + chunkContent,
//                Outline: currentOutline,
//                SectionPath: chunkLocation.SectionPath
//            ));
//        }

//        return result;
//    }

//    private static List<OutlineNode> ExtractMarkdownOutline(string text)
//    {
//        var outline = new List<OutlineNode>();
//        var lines = text.Split('\n');
//        var currentSection = new Stack<OutlineNode>();
//        int position = 0;

//        foreach (var line in lines)
//        {
//            int lineStart = position;
//            int lineEnd = position + line.Length;
//            position = lineEnd + 1; // +1 for newline

//            if (line.StartsWith("#"))
//            {
//                int level = line.TakeWhile(c => c == '#').Count();
//                string title = line[level..].Trim();

//                while (currentSection.Count > 0 && currentSection.Peek().Level >= level)
//                {
//                    currentSection.Pop();
//                }

//                var node = new OutlineNode(title, level, lineStart, lineEnd);

//                if (currentSection.Count > 0)
//                {
//                    currentSection.Peek().Children.Add(node);
//                }
//                else
//                {
//                    outline.Add(node);
//                }
//                currentSection.Push(node);
//            }
//        }
//        return outline;
//    }

//    private static List<OutlineNode> ExtractPlainTextOutline(string text)
//    {
//        var outline = new List<OutlineNode>();
//        var paragraphs = TextChunker.SplitPlainTextLines(text, int.MaxValue);
//        OutlineNode? currentParent = null;
//        int position = 0;

//        foreach (var para in paragraphs)
//        {
//            int paraStart = text.IndexOf(para, position, StringComparison.Ordinal);
//            if (paraStart == -1) continue;

//            int paraEnd = paraStart + para.Length;
//            position = paraEnd;

//            if (IsLikelyHeading(para))
//            {
//                var node = new OutlineNode(para.Trim(), 1, paraStart, paraEnd);

//                if (currentParent != null)
//                {
//                    currentParent.Children.Add(node);
//                }
//                else
//                {
//                    outline.Add(node);
//                }
//                currentParent = node;
//            }
//            else if (currentParent != null)
//            {
//                currentParent.EndPosition = paraEnd;
//            }
//        }
//        return outline;
//    }

//    private static bool IsLikelyHeading(string paragraph)
//    {
//        if (paragraph.Length > 150) return false;
//        if (paragraph.Contains('\n')) return false;

//        float titleCaseRatio = CalculateTitleCaseRatio(paragraph);
//        return titleCaseRatio > 0.6f ||
//               paragraph.EndsWith(":") ||
//               Regex.IsMatch(paragraph, @"^(Section|Chapter)\s+\d+");
//    }

//    private static float CalculateTitleCaseRatio(string text)
//    {
//        if (string.IsNullOrEmpty(text)) return 0;

//        int titleCaseCount = 0;
//        var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//        foreach (var word in words)
//        {
//            if (word.Length > 0 && char.IsUpper(word[0]))
//            {
//                titleCaseCount++;
//            }
//        }
//        return (float)titleCaseCount / words.Length;
//    }

//    private static (string SectionPath, string StartSection, string EndSection)
//        LocateChunkInOutline(List<OutlineNode> outline, int chunkStart, int chunkLength)
//    {
//        int chunkEnd = chunkStart + chunkLength;
//        var pathSegments = new List<string>();
//        var startSections = new List<string>();
//        var endSections = new List<string>();

//        void SearchNodes(List<OutlineNode> nodes)
//        {
//            foreach (var node in nodes)
//            {
//                if (chunkStart >= node.StartPosition && chunkStart <= node.EndPosition)
//                {
//                    pathSegments.Add(node.Title);
//                    startSections.Add(node.Title);

//                    if (chunkEnd <= node.EndPosition)
//                    {
//                        endSections.Add(node.Title);
//                    }

//                    SearchNodes(node.Children);
//                    return;
//                }
//            }
//        }

//        SearchNodes(outline);

//        if (startSections.Count == 0) startSections.Add("Document Beginning");
//        if (endSections.Count == 0) endSections.Add("Document End");

//        return (
//            string.Join(" > ", pathSegments),
//            string.Join(" > ", startSections),
//            string.Join(" > ", endSections)
//        );
//    }

//    private static string CompressOutline(List<OutlineNode> outline, int maxTokens, TextChunker.TokenCounter? counter)
//    {
//        var sb = new StringBuilder();
//        sb.AppendLine("Document Outline:");
//        foreach (var node in FlattenOutline(outline))
//        {
//            sb.AppendLine($"{new string(' ', node.Level * 2)}- {node.Title}");
//        }

//        return TruncateToTokenCount(sb.ToString(), maxTokens, counter);
//    }

//    private static IEnumerable<OutlineNode> FlattenOutline(List<OutlineNode> nodes)
//    {
//        foreach (var node in nodes)
//        {
//            yield return node;
//            foreach (var child in FlattenOutline(node.Children))
//            {
//                yield return child;
//            }
//        }
//    }

//    private static string TruncateToTokenCount(string text, int maxTokens, TextChunker.TokenCounter? counter)
//    {
//        int currentTokens = GetTokenCount(text, counter);
//        if (currentTokens <= maxTokens) return text;

//        int targetChars = maxTokens * 4;
//        return text.Length <= targetChars ?
//            text :
//            text[..targetChars] + "... [truncated]";
//    }

//    private static int GetTokenCount(string text, TextChunker.TokenCounter? counter)
//        => counter?.Invoke(text) ?? text.Length / 4;
//}
