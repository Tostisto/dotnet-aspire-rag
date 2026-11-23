using AspireRag.Domain.Common;
using AspireRag.Domain.Documents;

namespace AspireRag.Domain.Tests;

public sealed class DocumentIdTests
{
    [Theory]
    [InlineData("Vacation Policy", "vacation-policy")]
    [InlineData("remote_work (v2)", "remote-work-v2")]
    [InlineData("  IT & Security ", "it-security")]
    public void FromName_creates_a_slug(string name, string expected) =>
        DocumentId.FromName(name).Value.ShouldBe(expected);

    [Theory]
    [InlineData("")]
    [InlineData("Upper")]
    [InlineData("with space")]
    [InlineData("-leading")]
    public void From_rejects_invalid_ids(string value) =>
        Should.Throw<DomainException>(() => DocumentId.From(value));

    [Fact]
    public void FromName_rejects_names_without_any_slug_characters() =>
        Should.Throw<DomainException>(() => DocumentId.FromName("!!!"));
}
