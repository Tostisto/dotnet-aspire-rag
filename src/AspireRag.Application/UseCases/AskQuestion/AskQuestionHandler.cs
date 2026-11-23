using System.Diagnostics;
using AspireRag.Application.Abstractions;
using AspireRag.Domain.Conversations;
using Microsoft.Extensions.Options;

namespace AspireRag.Application.UseCases.AskQuestion;

public sealed class AskQuestionHandler(IAssistant assistant, IOptions<RagOptions> options)
{
    public async Task<AskQuestionResult> HandleAsync(AskQuestionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var question = Question.From(command.Question);
        var strategy = command.Strategy ?? options.Value.DefaultStrategy;

        var stopwatch = Stopwatch.StartNew();
        var reply = await assistant.AskAsync(question, strategy, cancellationToken).ConfigureAwait(false);

        return new AskQuestionResult(reply.Answer, strategy, reply.SearchCount, stopwatch.Elapsed);
    }
}
