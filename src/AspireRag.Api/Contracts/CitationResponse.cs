namespace AspireRag.Api.Contracts;

public sealed record CitationResponse(string DocumentId, string Title, Uri? Url, string Excerpt);
