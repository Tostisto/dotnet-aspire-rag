using AspireRag.Domain.Documents;

namespace AspireRag.Application.Abstractions;

public sealed record SearchHit(KnowledgeChunk Chunk, double Score);
