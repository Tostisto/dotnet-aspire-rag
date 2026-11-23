using System.Text;
using System.Text.RegularExpressions;
using AspireRag.Domain.Documents;

namespace AspireRag.Infrastructure.Knowledge;

internal sealed partial class MarkdownChunker(int maxChunkCharacters)
{
    private const int MaxHeadingLevel = 6;
    private const string PathSeparator = " > ";
    private const string ParagraphSeparator = "\n\n";
    private const string LineBreak = "\n";

    public IReadOnlyList<KnowledgeChunk> Chunk(SourceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return [.. ReadSections(document.Content)
            .SelectMany(section => ChunkSection(document.Title, section))
            .Select((text, index) => new KnowledgeChunk(document.Id, document.Title, index, text, document.Url))];
    }

    private IEnumerable<string> ChunkSection(string title, MarkdownSection section)
    {
        var path = section.Headings is [var first, ..] && first == title ? section.Headings : [title, .. section.Headings];
        var header = string.Join(PathSeparator, path) + LineBreak;
        var budget = Math.Max(maxChunkCharacters - header.Length, maxChunkCharacters / 2);

        var pieces = section.Paragraphs.SelectMany(paragraph =>
            paragraph.Length <= budget ? [paragraph] : SentenceBoundary().Split(paragraph));

        return Pack(pieces, budget).Select(body => header + body);
    }

    private static IEnumerable<MarkdownSection> ReadSections(string markdown)
    {
        var headings = new string?[MaxHeadingLevel];
        var paragraphs = new List<string>();
        var paragraph = new StringBuilder();
        var inCodeFence = false;

        foreach (var line in markdown.ReplaceLineEndings(LineBreak).Split(LineBreak))
        {
            inCodeFence ^= CodeFence().IsMatch(line);
            var heading = inCodeFence ? Match.Empty : HeadingPattern().Match(line);

            if (heading.Success)
            {
                EndParagraph(paragraph, paragraphs);
                if (TakeSection(headings, paragraphs) is { } section)
                {
                    yield return section;
                }

                var level = heading.Groups[1].Length;
                headings[level - 1] = heading.Groups[2].Value.Trim();
                Array.Clear(headings, level, MaxHeadingLevel - level);
            }
            else if (!inCodeFence && string.IsNullOrWhiteSpace(line))
            {
                EndParagraph(paragraph, paragraphs);
            }
            else
            {
                paragraph.AppendLine(line);
            }
        }

        EndParagraph(paragraph, paragraphs);
        if (TakeSection(headings, paragraphs) is { } last)
        {
            yield return last;
        }
    }

    private static void EndParagraph(StringBuilder paragraph, List<string> paragraphs)
    {
        if (paragraph.Length == 0)
        {
            return;
        }

        paragraphs.Add(paragraph.ToString().Trim());
        paragraph.Clear();
    }

    private static MarkdownSection? TakeSection(string?[] headings, List<string> paragraphs)
    {
        if (paragraphs.Count == 0)
        {
            return null;
        }

        var section = new MarkdownSection([.. headings.OfType<string>()], [.. paragraphs]);
        paragraphs.Clear();
        return section;
    }

    private static IEnumerable<string> Pack(IEnumerable<string> pieces, int budget)
    {
        var current = new StringBuilder();
        foreach (var piece in pieces)
        {
            if (current.Length > 0 && current.Length + ParagraphSeparator.Length + piece.Length > budget)
            {
                yield return current.ToString();
                current.Clear();
            }

            current.Append(current.Length > 0 ? ParagraphSeparator : string.Empty).Append(piece);
        }

        if (current.Length > 0)
        {
            yield return current.ToString();
        }
    }

    [GeneratedRegex(@"^\s*(?:\d+\.\s+)?(#{1,6})\s+(.+)$")]
    private static partial Regex HeadingPattern();

    [GeneratedRegex(@"^\s*(```|~~~)")]
    private static partial Regex CodeFence();

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceBoundary();
}
