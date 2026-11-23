using AspireRag.Domain.Documents;

namespace AspireRag.Domain.Conversations;

public sealed record Citation(DocumentId DocumentId, string Title, string Excerpt, Uri? Url = null)
{
    public const int MaxExcerptLength = 240;
    private const string Ellipsis = "…";

    public static Citation FromChunk(KnowledgeChunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        var excerpt = chunk.Text.Length <= MaxExcerptLength
            ? chunk.Text
            : string.Concat(chunk.Text.AsSpan(0, MaxExcerptLength).TrimEnd(), Ellipsis);
        return new Citation(chunk.DocumentId, chunk.DocumentTitle, excerpt, chunk.SourceUrl);
    }
}
