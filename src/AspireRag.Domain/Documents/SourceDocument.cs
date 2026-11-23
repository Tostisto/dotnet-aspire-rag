using AspireRag.Domain.Common;

namespace AspireRag.Domain.Documents;

public sealed record SourceDocument
{
    private const int MaxContentLength = 200_000;
    private const string EmptyTitleMessage = "Document title must not be empty.";

    public SourceDocument(DocumentId id, string title, string content, Uri? url = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException(EmptyTitleMessage);
        }

        if (string.IsNullOrWhiteSpace(content) || content.Length > MaxContentLength)
        {
            throw new DomainException($"Document '{id}' must have between 1 and {MaxContentLength} characters of content.");
        }

        Id = id;
        Title = title.Trim();
        Content = content;
        Url = url;
    }

    public DocumentId Id { get; }

    public string Title { get; }

    public string Content { get; }

    public Uri? Url { get; }
}
