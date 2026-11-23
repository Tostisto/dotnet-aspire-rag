using AspireRag.Application.Abstractions;
using AspireRag.Domain.Documents;

namespace AspireRag.Application.Tests;

internal sealed class FakeDocumentSource(params SourceDocument[] documents) : IDocumentSource
{
    public IAsyncEnumerable<SourceDocument> GetDocumentsAsync(CancellationToken cancellationToken = default) =>
        documents.ToAsyncEnumerable();
}
