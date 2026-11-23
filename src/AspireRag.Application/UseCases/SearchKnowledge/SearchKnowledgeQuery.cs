namespace AspireRag.Application.UseCases.SearchKnowledge;

public sealed record SearchKnowledgeQuery(string? Query, int? Top = null);
