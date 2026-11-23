using AspireRag.Infrastructure.Documents;

namespace AspireRag.Infrastructure.Tests;

public sealed class MarkdownCleanerTests
{
    private const string Page =
        """
        ---
        title: "Contributing"
        ---

        Install the <code>winget</code> package.

        <details><summary><b>Windows</b></summary>
        Run the installer for <VERSION>.
        </details>



        ```xml
        <PackageReference Include="Aspire.Hosting.Redis" />
        ```

          ```bash
          # restore first
          <b>not html here</b>
          ````
        """;

    [Fact]
    public void Title_comes_from_front_matter() =>
        MarkdownCleaner.ReadTitle(Page).ShouldBe("Contributing");

    [Fact]
    public void Title_falls_back_to_the_first_heading() =>
        MarkdownCleaner.ReadTitle("Intro\r\n\r\n# Coding Guidelines\r\n\r\nText").ShouldBe("Coding Guidelines");

    [Fact]
    public void Front_matter_and_html_are_removed_from_prose()
    {
        var markdown = MarkdownCleaner.Clean(Page);

        markdown.ShouldStartWith("Install the `winget` package.");
        markdown.ShouldContain("Windows\nRun the installer for <VERSION>.");
        markdown.ShouldNotContain("title:");
        markdown.ShouldNotContain("<details>");
        markdown.ShouldNotContain("\n\n\n");
    }

    [Fact]
    public void Code_blocks_are_kept_unchanged()
    {
        var markdown = MarkdownCleaner.Clean(Page);

        markdown.ShouldContain("""<PackageReference Include="Aspire.Hosting.Redis" />""");
        markdown.ShouldContain("# restore first\n  <b>not html here</b>");
    }
}
