namespace AspireRag.IntegrationTests;

public sealed record AskResponse(string Answer, IReadOnlyList<CitationResponse> Citations, string Mode, int Searches, long ElapsedMilliseconds);
