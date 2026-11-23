using AspireRag.Domain.Common;
using AspireRag.Domain.Conversations;
using AspireRag.Domain.Documents;

namespace AspireRag.Domain.Tests;

public sealed class ConversationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Question_must_not_be_empty(string? text) =>
        Should.Throw<DomainException>(() => Question.From(text));

    [Fact]
    public void Question_is_length_limited() =>
        Should.Throw<DomainException>(() => Question.From(new string('a', Question.MaxLength + 1)));

    [Fact]
    public void Question_is_trimmed() =>
        Question.From("  How many vacation days?  ").Text.ShouldBe("How many vacation days?");

    [Fact]
    public void Citation_excerpt_is_truncated()
    {
        var chunk = new KnowledgeChunk(DocumentId.From("doc"), "Doc", 0, new string('x', 1000));

        var citation = Citation.FromChunk(chunk);

        citation.Excerpt.Length.ShouldBe(Citation.MaxExcerptLength + 1);
        citation.Excerpt.ShouldEndWith("…");
    }

    [Fact]
    public void Answer_cites_only_retrieved_documents_it_mentions_once_each()
    {
        var vacation = DocumentId.From("vacation");
        var expenses = DocumentId.From("expenses");
        var remote = DocumentId.From("remote");

        var answer = Answer.Create(
            " 5 days [Vacation Policy], 45 EUR [travel and expenses] ",
            [new(expenses, "Travel and Expenses", "1"), new(remote, "Remote Work", "2"),
             new(vacation, "Vacation Policy", "3"), new(expenses, "Travel and Expenses", "4")]);

        answer.Text.ShouldBe("5 days [Vacation Policy], 45 EUR [travel and expenses]");
        answer.Citations.Select(c => c.DocumentId).ShouldBe([expenses, vacation]);
    }
}
