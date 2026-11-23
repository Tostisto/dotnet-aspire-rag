using AspireRag.Application.Abstractions;
using AspireRag.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AspireRag.Application.UseCases.IngestDocuments;

public sealed partial class IngestDocumentsHandler(
    IDocumentSource source,
    IKnowledgeBase knowledgeBase,
    ILogger<IngestDocumentsHandler> logger)
{
    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        await knowledgeBase.ClearAsync(cancellationToken).ConfigureAwait(false);

        var documents = 0;
        var chunks = 0;

        await foreach (var document in source.GetDocumentsAsync(cancellationToken).ConfigureAwait(false))
        {
            var stored = await knowledgeBase.IngestAsync(document, cancellationToken).ConfigureAwait(false);
            LogIngested(document.Id, stored);
            documents++;
            chunks += stored;
        }

        LogFinished(documents, chunks);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Ingested document {DocumentId} as {Chunks} chunks")]
    private partial void LogIngested(DocumentId documentId, int chunks);

    [LoggerMessage(Level = LogLevel.Information, Message = "Ingestion finished: {Documents} documents, {Chunks} chunks")]
    private partial void LogFinished(int documents, int chunks);
}
