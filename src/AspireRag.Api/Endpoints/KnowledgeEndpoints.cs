using AspireRag.Api.Contracts;
using AspireRag.Application.UseCases.AskQuestion;
using AspireRag.Application.UseCases.SearchKnowledge;

namespace AspireRag.Api.Endpoints;

internal static class KnowledgeEndpoints
{
    private const string GroupRoute = "/api";
    private const string AskRoute = "/ask";
    private const string SearchRoute = "/search";
    private const string Tag = "Knowledge";
    private const string AskSummary = "Ask the assistant a question answered from the knowledge base.";
    private const string SearchSummary = "Semantic search over the knowledge base without the LLM.";
    private const int ScoreDecimals = 4;

    public static IEndpointRouteBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup(GroupRoute).WithTags(Tag);

        api.MapPost(AskRoute, AskAsync)
            .WithSummary(AskSummary)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        api.MapGet(SearchRoute, SearchAsync)
            .WithSummary(SearchSummary)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<AskResponse> AskAsync(AskRequest request, AskQuestionHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new AskQuestionCommand(request.Question, request.Mode), cancellationToken);

        return new AskResponse(
            result.Answer.Text,
            [.. result.Answer.Citations.Select(static c => new CitationResponse(c.DocumentId.Value, c.Title, c.Url, c.Excerpt))],
            result.Strategy,
            result.SearchCount,
            (long)result.Elapsed.TotalMilliseconds);
    }

    private static async Task<IReadOnlyList<SearchHitResponse>> SearchAsync(
        string? q, int? top, SearchKnowledgeHandler handler, CancellationToken cancellationToken)
    {
        var hits = await handler.HandleAsync(new SearchKnowledgeQuery(q, top), cancellationToken);

        return [.. hits.Select(static h => new SearchHitResponse(
            h.Chunk.DocumentId.Value, h.Chunk.DocumentTitle, h.Chunk.SourceUrl, h.Chunk.Index, Math.Round(h.Score, ScoreDecimals), h.Chunk.Text))];
    }
}
