using AspireRag.Domain.Documents;
using Microsoft.Extensions.VectorData;

namespace AspireRag.Infrastructure.Knowledge;

internal sealed class ChunkRecord
{
    public Guid Id { get; init; }

    public string DocumentId { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    public int ChunkIndex { get; init; }

    public string Text { get; init; } = string.Empty;

    public string? SourceUrl { get; init; }

    public string Embedding { get; init; } = string.Empty;

    public static ChunkRecord From(KnowledgeChunk chunk, string embeddingPrefix) => new()
    {
        Id = Guid.CreateVersion7(),
        DocumentId = chunk.DocumentId.Value,
        DocumentTitle = chunk.DocumentTitle,
        ChunkIndex = chunk.Index,
        Text = chunk.Text,
        SourceUrl = chunk.SourceUrl?.ToString(),
        Embedding = embeddingPrefix + chunk.Text,
    };

    public static VectorStoreCollectionDefinition CreateDefinition(int dimensions) => new()
    {
        Properties =
        [
            new VectorStoreKeyProperty(nameof(Id), typeof(Guid)),
            new VectorStoreDataProperty(nameof(DocumentId), typeof(string)) { IsIndexed = true },
            new VectorStoreDataProperty(nameof(DocumentTitle), typeof(string)),
            new VectorStoreDataProperty(nameof(ChunkIndex), typeof(int)),
            new VectorStoreDataProperty(nameof(Text), typeof(string)),
            new VectorStoreDataProperty(nameof(SourceUrl), typeof(string)),
            new VectorStoreVectorProperty(nameof(Embedding), typeof(string), dimensions)
            {
                DistanceFunction = DistanceFunction.CosineSimilarity,
            },
        ],
    };

    public KnowledgeChunk ToChunk() =>
        new(Domain.Documents.DocumentId.From(DocumentId), DocumentTitle, ChunkIndex, Text, SourceUrl is null ? null : new Uri(SourceUrl));
}
