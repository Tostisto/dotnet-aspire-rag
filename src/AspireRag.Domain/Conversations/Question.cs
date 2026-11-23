using AspireRag.Domain.Common;

namespace AspireRag.Domain.Conversations;

public sealed record Question
{
    public const int MaxLength = 1_000;

    private Question(string text) => Text = text;

    public string Text { get; }

    public static Question From(string? text)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxLength)
        {
            throw new DomainException($"Question must have between 1 and {MaxLength} characters.");
        }

        return new Question(trimmed);
    }
}
