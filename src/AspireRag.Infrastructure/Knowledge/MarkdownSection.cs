namespace AspireRag.Infrastructure.Knowledge;

internal sealed record MarkdownSection(IReadOnlyList<string> Headings, IReadOnlyList<string> Paragraphs);
