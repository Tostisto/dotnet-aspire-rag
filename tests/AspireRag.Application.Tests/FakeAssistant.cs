using AspireRag.Application.Abstractions;
using AspireRag.Domain.Conversations;

namespace AspireRag.Application.Tests;

internal sealed class FakeAssistant : IAssistant
{
    public List<(Question Question, RetrievalStrategy Strategy)> Calls { get; } = [];

    public Task<AssistantReply> AskAsync(Question question, RetrievalStrategy strategy, CancellationToken cancellationToken = default)
    {
        Calls.Add((question, strategy));
        return Task.FromResult(new AssistantReply(Answer.Create("42", []), SearchCount: 1));
    }
}
