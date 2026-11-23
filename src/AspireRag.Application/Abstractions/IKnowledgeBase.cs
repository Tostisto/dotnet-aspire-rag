using AspireRag.Domain.Documents;

namespace AspireRag.Application.Abstractions;

public interface IKnowledgeBase
{
    Task ClearAsync(CancellationToken cancellationToken = default);

    Task<int> IngestAsync(SourceDocument document, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top, CancellationToken cancellationToken = default);
}
