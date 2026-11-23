using AspireRag.Domain.Conversations;

namespace AspireRag.Application.Abstractions;

public interface IAssistant
{
    Task<AssistantReply> AskAsync(Question question, RetrievalStrategy strategy, CancellationToken cancellationToken = default);
}
