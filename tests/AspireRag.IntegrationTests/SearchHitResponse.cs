namespace AspireRag.IntegrationTests;

public sealed record SearchHitResponse(string DocumentId, string Title, Uri? Url, int ChunkIndex, double Score, string Text);
