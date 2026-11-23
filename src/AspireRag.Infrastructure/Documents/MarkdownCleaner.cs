using System.Text;
using System.Text.RegularExpressions;

namespace AspireRag.Infrastructure.Documents;

internal static partial class MarkdownCleaner
{
    private const string LineBreak = "\n";
    private const string ParagraphBreak = "\n\n";
    private const string BacktickFence = "```";
    private const string TildeFence = "~~~";
    private const string InlineCodeReplacement = "`$1`";

    private static readonly char[] Indentation = [' ', '\t'];
    private static readonly char[] Quotes = ['"', '\''];

    public static string? ReadTitle(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var text = markdown.ReplaceLineEndings(LineBreak);
        var title = FrontMatterTitle().Match(FrontMatter().Match(text).Value);
        if (title.Success)
        {
            return title.Groups[1].Value.Trim().Trim(Quotes);
        }

        var heading = Heading().Match(text);
        return heading.Success ? heading.Groups[1].Value.Trim() : null;
    }

    public static string Clean(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var text = FrontMatter().Replace(markdown.ReplaceLineEndings(LineBreak), string.Empty, 1);
        var result = new StringBuilder(text.Length);

        foreach (var segment in CodeFence().Split(text))
        {
            var fence = segment.TrimStart(Indentation);
            if (fence.StartsWith(BacktickFence, StringComparison.Ordinal) || fence.StartsWith(TildeFence, StringComparison.Ordinal))
            {
                result.Append(segment);
                continue;
            }

            var prose = InlineCodeTag().Replace(segment, InlineCodeReplacement);
            result.Append(HtmlTag().Replace(prose, string.Empty));
        }

        return BlankLines().Replace(result.ToString(), ParagraphBreak).Trim();
    }

    [GeneratedRegex(@"\A---\n.*?\n---\n", RegexOptions.Singleline)]
    private static partial Regex FrontMatter();

    [GeneratedRegex(@"^title:\s*(.+)$", RegexOptions.Multiline)]
    private static partial Regex FrontMatterTitle();

    [GeneratedRegex(@"^#\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex(@"(^[ \t]*(?:`{3,}|~{3,}).*?^[ \t]*(?:`{3,}|~{3,})[ \t]*$)", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex CodeFence();

    [GeneratedRegex(@"<code>(.*?)</code>")]
    private static partial Regex InlineCodeTag();

    [GeneratedRegex(@"</?(?:a|abbr|b|i|u|div|p|span|br|img|strong|em|ul|ol|li|table|thead|tbody|tr|th|td|details|summary|kbd|sup|sub|h[1-6])\b[^<>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTag();

    [GeneratedRegex(@"\n[ \t]*(\n[ \t]*)+")]
    private static partial Regex BlankLines();
}
