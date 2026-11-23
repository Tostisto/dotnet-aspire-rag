using AspireRag.Domain.Documents;
using AspireRag.Infrastructure.Knowledge;

namespace AspireRag.Infrastructure.Tests;

public sealed class MarkdownChunkerTests
{
    private static SourceDocument Doc(string markdown) => new(DocumentId.From("policy"), "Vacation Policy", markdown);

    [Fact]
    public void Each_chunk_carries_its_heading_path()
    {
        var markdown = """
            # Vacation Policy
            Intro text.

            ## Carry-over
            Up to 5 days.

            ### Expiry
            They expire on March 31.

            ## Requesting
            Use the portal.
            """;

        var chunks = new MarkdownChunker(1000).Chunk(Doc(markdown));

        chunks.Select(c => c.Text).ShouldBe(
        [
            "Vacation Policy\nIntro text.",
            "Vacation Policy > Carry-over\nUp to 5 days.",
            "Vacation Policy > Carry-over > Expiry\nThey expire on March 31.",
            "Vacation Policy > Requesting\nUse the portal.",
        ]);
        chunks.Select(c => c.Index).ShouldBe([0, 1, 2, 3]);
        chunks.ShouldAllBe(c => c.DocumentId == DocumentId.From("policy") && c.DocumentTitle == "Vacation Policy");
    }

    [Fact]
    public void Sibling_headings_replace_each_other()
    {
        var markdown = """
            ## Intro
            Guidelines.

            ## Indentation
            We use tabs.
            """;

        new MarkdownChunker(1000).Chunk(Doc(markdown)).Select(static c => c.Text).ShouldBe(
        [
            "Vacation Policy > Intro\nGuidelines.",
            "Vacation Policy > Indentation\nWe use tabs.",
        ]);
    }

    [Fact]
    public void Paragraphs_are_packed_up_to_the_size_limit()
    {
        var paragraphs = Enumerable.Range(1, 30).Select(i => $"Paragraph number {i} with some filler text in it.");
        var markdown = "# Title\n\n" + string.Join("\n\n", paragraphs);

        var chunks = new MarkdownChunker(300).Chunk(Doc(markdown));

        chunks.Count.ShouldBeGreaterThan(1);
        chunks.ShouldAllBe(c => c.Text.Length <= 300);
        string.Concat(chunks.Select(c => c.Text)).ShouldContain("Paragraph number 30 ");
        chunks.Sum(c => c.Text.Split("Paragraph number").Length - 1).ShouldBe(30);
    }

    [Fact]
    public void A_very_long_paragraph_is_split_on_sentences()
    {
        var sentences = string.Join(" ", Enumerable.Range(1, 40).Select(i => $"This is sentence {i}."));

        var chunks = new MarkdownChunker(200).Chunk(Doc(sentences));

        chunks.Count.ShouldBeGreaterThan(1);
        chunks.ShouldAllBe(c => c.Text.Length <= 200);
        chunks.ShouldAllBe(c => c.Text.EndsWith('.'));
    }

    [Fact]
    public void Code_blocks_stay_whole_and_chunks_start_with_the_page_title()
    {
        var markdown = """
            # Install

            1. ### Add the package

            ```bash
            # restore first

            dotnet restore
            ```
            """;

        var chunks = new MarkdownChunker(1000).Chunk(Doc(markdown));

        chunks.ShouldHaveSingleItem().Text.ShouldBe(
            "Vacation Policy > Install > Add the package\n```bash\n# restore first\n\ndotnet restore\n```");
    }

    [Fact]
    public void Chunks_carry_the_document_url()
    {
        var url = new Uri("https://aspire.dev/get-started/");
        var document = new SourceDocument(DocumentId.From("get-started"), "Get started", "# Title\n\nText", url);

        new MarkdownChunker(1000).Chunk(document).ShouldAllBe(c => c.SourceUrl == url);
    }

}
