namespace AspireRag.IntegrationTests;

public sealed record CitationResponse(string DocumentId, string Title, Uri? Url, string Excerpt);
