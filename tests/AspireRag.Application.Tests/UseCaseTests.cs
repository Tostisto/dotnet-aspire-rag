using AspireRag.Application.Abstractions;
using AspireRag.Application.UseCases.AskQuestion;
using AspireRag.Application.UseCases.IngestDocuments;
using AspireRag.Application.UseCases.SearchKnowledge;
using AspireRag.Domain.Common;
using AspireRag.Domain.Documents;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AspireRag.Application.Tests;

public sealed class UseCaseTests
{
    private static readonly IOptions<RagOptions> Options =
        Microsoft.Extensions.Options.Options.Create(new RagOptions { DefaultStrategy = RetrievalStrategy.OnDemand, TopK = 3 });

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Ask_uses_the_configured_default_strategy()
    {
        var assistant = new FakeAssistant();

        var result = await new AskQuestionHandler(assistant, Options).HandleAsync(new("How many vacation days?"), Ct);

        assistant.Calls.ShouldHaveSingleItem().Strategy.ShouldBe(RetrievalStrategy.OnDemand);
        result.Strategy.ShouldBe(RetrievalStrategy.OnDemand);
        result.Answer.Text.ShouldBe("42");
        result.SearchCount.ShouldBe(1);
    }

    [Fact]
    public async Task Ask_honours_an_explicit_strategy()
    {
        var assistant = new FakeAssistant();

        await new AskQuestionHandler(assistant, Options).HandleAsync(new("Q?", RetrievalStrategy.Always), Ct);

        assistant.Calls.ShouldHaveSingleItem().Strategy.ShouldBe(RetrievalStrategy.Always);
    }

    [Fact]
    public async Task Ask_rejects_an_invalid_question_without_calling_the_model()
    {
        var assistant = new FakeAssistant();

        await Should.ThrowAsync<DomainException>(() => new AskQuestionHandler(assistant, Options).HandleAsync(new("  "), Ct));

        assistant.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Ingest_rebuilds_the_knowledge_base_from_every_document()
    {
        var knowledgeBase = new FakeKnowledgeBase();
        var source = new FakeDocumentSource(
            new SourceDocument(DocumentId.From("a"), "A", "1234"),
            new SourceDocument(DocumentId.From("b"), "B", "56"));

        await new IngestDocumentsHandler(source, knowledgeBase, NullLogger<IngestDocumentsHandler>.Instance).HandleAsync(Ct);

        knowledgeBase.Cleared.ShouldBeTrue();
        knowledgeBase.Ingested.Select(static d => d.Id.Value).ShouldBe(["a", "b"]);
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(0, 1)]
    [InlineData(500, RagOptions.MaxTopK)]
    public async Task Search_defaults_and_clamps_top(int? requested, int expected)
    {
        var knowledgeBase = new FakeKnowledgeBase();

        await new SearchKnowledgeHandler(knowledgeBase, Options).HandleAsync(new(" vacation ", requested), Ct);

        knowledgeBase.Searches.ShouldHaveSingleItem().ShouldBe(("vacation", expected));
    }
}
