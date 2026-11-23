namespace AspireRag.Domain.Documents;

public sealed record KnowledgeChunk(DocumentId DocumentId, string DocumentTitle, int Index, string Text, Uri? SourceUrl = null);
