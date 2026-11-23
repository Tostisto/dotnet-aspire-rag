using AspireRag.Domain.Documents;

namespace AspireRag.Application.Abstractions;

public interface IDocumentSource
{
    IAsyncEnumerable<SourceDocument> GetDocumentsAsync(CancellationToken cancellationToken = default);
}
