using System.ComponentModel.DataAnnotations;
using AspireRag.Application.Abstractions;

namespace AspireRag.Application;

public sealed class RagOptions
{
    public const string SectionName = "Rag";
    public const int MinTopK = 1;
    public const int MaxTopK = 20;
    private const int DefaultTopK = 4;

    public RetrievalStrategy DefaultStrategy { get; set; } = RetrievalStrategy.Always;

    [Range(MinTopK, MaxTopK)]
    public int TopK { get; set; } = DefaultTopK;
}
