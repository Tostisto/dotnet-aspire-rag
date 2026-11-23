using AspireRag.Application.Abstractions;
using AspireRag.Domain.Conversations;
using Microsoft.Extensions.Options;

namespace AspireRag.Application.UseCases.SearchKnowledge;

public sealed class SearchKnowledgeHandler(IKnowledgeBase knowledgeBase, IOptions<RagOptions> options)
{
    public Task<IReadOnlyList<SearchHit>> HandleAsync(SearchKnowledgeQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var text = Question.From(query.Query).Text;
        var top = Math.Clamp(query.Top ?? options.Value.TopK, RagOptions.MinTopK, RagOptions.MaxTopK);

        return knowledgeBase.SearchAsync(text, top, cancellationToken);
    }
}
