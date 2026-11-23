using AspireRag.Application.Abstractions;
using AspireRag.Domain.Documents;

namespace AspireRag.Application.Tests;

internal sealed class FakeKnowledgeBase : IKnowledgeBase
{
    public bool Cleared { get; private set; }

    public List<SourceDocument> Ingested { get; } = [];

    public List<(string Query, int Top)> Searches { get; } = [];

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        Cleared = Ingested.Count == 0;
        return Task.CompletedTask;
    }

    public Task<int> IngestAsync(SourceDocument document, CancellationToken cancellationToken = default)
    {
        Ingested.Add(document);
        return Task.FromResult(document.Content.Length);
    }

    public Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top, CancellationToken cancellationToken = default)
    {
        Searches.Add((query, top));
        return Task.FromResult<IReadOnlyList<SearchHit>>([]);
    }
}
