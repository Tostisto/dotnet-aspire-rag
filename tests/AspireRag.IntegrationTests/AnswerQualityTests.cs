using System.Net.Http.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AspireRag.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Category", "Evaluation")]
public sealed class AnswerQualityTests(AppFixture fixture)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("How should types and functions be named?", "Always")]
    [InlineData("How should types and functions be named?", "OnDemand")]
    [InlineData("Which quotes should be used for strings shown to the user?", "OnDemand")]
    public async Task Answers_are_grounded_in_the_retrieved_documents(string question, string mode)
    {
        using var response = await fixture.Api.PostAsJsonAsync("/api/ask", new { question, mode }, Ct);
        response.EnsureSuccessStatusCode();
        var answer = (await response.Content.ReadFromJsonAsync<AskResponse>(Ct)).ShouldNotBeNull();

        var context = (await fixture.Api.GetFromJsonAsync<SearchHitResponse[]>($"/api/search?q={Uri.EscapeDataString(question)}&top=4", Ct)).ShouldNotBeNull();
        var groundingText = string.Join("\n\n", context.Select(static h => h.Text));

        var result = await new GroundednessEvaluator().EvaluateAsync(
            [new ChatMessage(ChatRole.User, question)],
            new ChatResponse(new ChatMessage(ChatRole.Assistant, answer.Answer)),
            new ChatConfiguration(fixture.ChatModel),
            [new GroundednessEvaluatorContext(groundingText)],
            Ct);

        var groundedness = result.Get<NumericMetric>(GroundednessEvaluator.GroundednessMetricName);
        groundedness.Value.ShouldNotBeNull();
        groundedness.Value.Value.ShouldBeGreaterThanOrEqualTo(3, $"Answer: {answer.Answer}\nReason: {groundedness.Reason}");
    }
}
