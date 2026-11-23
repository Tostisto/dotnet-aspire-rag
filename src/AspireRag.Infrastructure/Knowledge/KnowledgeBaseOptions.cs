using System.ComponentModel.DataAnnotations;

namespace AspireRag.Infrastructure.Knowledge;

public sealed class KnowledgeBaseOptions
{
    public const string SectionName = "KnowledgeBase";
    private const string DefaultCollectionName = "docs";
    private const int DefaultEmbeddingDimensions = 768;
    private const int MaxEmbeddingDimensions = 8192;
    private const int DefaultChunkSize = 1200;
    private const int MinChunkSize = 200;
    private const int MaxChunkSize = 8000;

    [Required]
    public string CollectionName { get; set; } = DefaultCollectionName;

    [Range(1, MaxEmbeddingDimensions)]
    public int EmbeddingDimensions { get; set; } = DefaultEmbeddingDimensions;

    public string DocumentEmbeddingPrefix { get; set; } = string.Empty;

    public string QueryEmbeddingPrefix { get; set; } = string.Empty;

    [Range(MinChunkSize, MaxChunkSize)]
    public int MaxChunkCharacters { get; set; } = DefaultChunkSize;
}
