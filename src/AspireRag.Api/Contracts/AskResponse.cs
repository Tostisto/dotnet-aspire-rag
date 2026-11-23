using AspireRag.Application.Abstractions;

namespace AspireRag.Api.Contracts;

public sealed record AskResponse(
    string Answer,
    IReadOnlyList<CitationResponse> Citations,
    RetrievalStrategy Mode,
    int Searches,
    long ElapsedMilliseconds);
