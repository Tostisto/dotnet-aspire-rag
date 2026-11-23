using AspireRag.Application;
using AspireRag.Application.Abstractions;
using AspireRag.Application.Policies;
using AspireRag.Domain.Conversations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Microsoft.Agents.AI.TextSearchProvider;
using static Microsoft.Agents.AI.TextSearchProviderOptions;

namespace AspireRag.Infrastructure.Assistant;

internal sealed class AgentAssistant(
    IChatClient chatClient,
    IKnowledgeBase knowledgeBase,
    IOptions<RagOptions> ragOptions,
    ILoggerFactory loggerFactory) : IAssistant
{
    private const string TelemetrySourceName = "AspireRag.Assistant";
    private const float Temperature = 0f;
    private const long Seed = 42;

    public async Task<AssistantReply> AskAsync(Question question, RetrievalStrategy strategy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);

        var citations = new List<Citation>();
        var searchCount = 0;

        var agent = CreateAgent(strategy, SearchAsync);
        var response = await agent.RunAsync(question.Text, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new AssistantReply(Answer.Create(response.Text, citations), searchCount);

        async Task<IEnumerable<TextSearchResult>> SearchAsync(string query, CancellationToken token)
        {
            var hits = await knowledgeBase.SearchAsync(query, ragOptions.Value.TopK, token).ConfigureAwait(false);
            searchCount++;
            citations.AddRange(hits.Select(static hit => Citation.FromChunk(hit.Chunk)));
            return hits.Select(static hit => new TextSearchResult
            {
                SourceName = hit.Chunk.DocumentTitle,
                SourceLink = hit.Chunk.SourceUrl?.ToString(),
                Text = hit.Chunk.Text,
            });
        }
    }

    private AIAgent CreateAgent(
        RetrievalStrategy strategy,
        Func<string, CancellationToken, Task<IEnumerable<TextSearchResult>>> search)
    {
        var searchProvider = new TextSearchProvider(
            search,
            new TextSearchProviderOptions
            {
                SearchTime = strategy is RetrievalStrategy.OnDemand
                    ? TextSearchBehavior.OnDemandFunctionCalling
                    : TextSearchBehavior.BeforeAIInvoke,
                FunctionToolName = AssistantPolicy.SearchToolName,
                FunctionToolDescription = AssistantPolicy.SearchToolDescription,
                CitationsPrompt = AssistantPolicy.CitationsPrompt,
            },
            loggerFactory);

        var options = new ChatClientAgentOptions
        {
            Name = AssistantPolicy.Name,
            ChatOptions = new ChatOptions { Instructions = AssistantPolicy.Instructions, Temperature = Temperature, Seed = Seed },
            AIContextProviders = [searchProvider],
        };

        return chatClient.AsAIAgent(options, loggerFactory)
            .AsBuilder()
            .UseOpenTelemetry(TelemetrySourceName)
            .Build();
    }
}
