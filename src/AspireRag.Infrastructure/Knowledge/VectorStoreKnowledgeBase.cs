using AspireRag.Application.Abstractions;
using AspireRag.Domain.Documents;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace AspireRag.Infrastructure.Knowledge;

internal sealed class VectorStoreKnowledgeBase(
    VectorStoreCollection<Guid, ChunkRecord> collection,
    MarkdownChunker chunker,
    IOptions<KnowledgeBaseOptions> options) : IKnowledgeBase
{
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await collection.EnsureCollectionDeletedAsync(cancellationToken).ConfigureAwait(false);
        await collection.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> IngestAsync(SourceDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        await collection.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);

        var prefix = options.Value.DocumentEmbeddingPrefix;
        var records = chunker.Chunk(document).Select(chunk => ChunkRecord.From(chunk, prefix)).ToList();

        await collection.UpsertAsync(records, cancellationToken).ConfigureAwait(false);
        return records.Count;
    }

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!await collection.CollectionExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            return [];
        }

        return await collection
            .SearchAsync(options.Value.QueryEmbeddingPrefix + query, top, cancellationToken: cancellationToken)
            .Select(static result => new SearchHit(result.Record.ToChunk(), result.Score ?? 0))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
