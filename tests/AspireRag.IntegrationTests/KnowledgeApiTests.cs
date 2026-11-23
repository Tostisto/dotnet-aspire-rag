using System.Net.Http.Json;

namespace AspireRag.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class KnowledgeApiTests(AppFixture fixture)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Search_finds_the_relevant_document()
    {
        var hits = await fixture.Api.GetFromJsonAsync<SearchHitResponse[]>(
            "/api/search?q=Do we indent with tabs or spaces?&top=3", Ct);

        hits.ShouldNotBeNull().ShouldNotBeEmpty();
        hits[0].DocumentId.ShouldBe("vscode-wiki-coding-guidelines");
        hits[0].Url.ShouldBe(new Uri("https://github.com/microsoft/vscode/wiki/Coding-Guidelines"));
    }

    [Theory]
    [InlineData("Always")]
    [InlineData("OnDemand")]
    public async Task Ask_answers_from_the_knowledge_base_with_citations(string mode)
    {
        var answer = await AskAsync("Do we indent code with tabs or spaces?", mode);

        answer.Mode.ShouldBe(mode);
        answer.Answer.ShouldContain("tabs", Case.Insensitive);
        answer.Searches.ShouldBeGreaterThanOrEqualTo(1);
        answer.Citations.ShouldContain(c => c.DocumentId == "vscode-wiki-coding-guidelines");
    }

    [Fact]
    public async Task Ask_rejects_an_empty_question()
    {
        using var response = await fixture.Api.PostAsJsonAsync("/api/ask", new { question = string.Empty }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<AskResponse> AskAsync(string question, string mode)
    {
        using var response = await fixture.Api.PostAsJsonAsync("/api/ask", new { question, mode }, Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AskResponse>(Ct)).ShouldNotBeNull();
    }
}
