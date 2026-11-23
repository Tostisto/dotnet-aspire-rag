namespace AspireRag.Domain.Conversations;

public sealed record Answer(string Text, IReadOnlyList<Citation> Citations)
{
    public static Answer Create(string text, IEnumerable<Citation> retrieved)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(retrieved);

        var cited = retrieved
            .DistinctBy(static c => c.DocumentId)
            .Where(c => text.Contains(c.Title, StringComparison.OrdinalIgnoreCase));

        return new Answer(text.Trim(), [.. cited]);
    }
}
