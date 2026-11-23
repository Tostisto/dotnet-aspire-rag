using System.Text.RegularExpressions;
using AspireRag.Domain.Common;

namespace AspireRag.Domain.Documents;

public readonly partial record struct DocumentId
{
    private const int MaxLength = 100;
    private const string SlugSeparator = "-";

    private DocumentId(string value) => Value = value;

    public string Value { get; }

    public static DocumentId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength || !SlugPattern().IsMatch(value))
        {
            throw new DomainException($"Document id '{value}' must be a lowercase slug of at most {MaxLength} characters.");
        }

        return new DocumentId(value);
    }

    public static DocumentId FromName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var slug = NonSlugCharacters().Replace(name.ToLowerInvariant(), SlugSeparator).Trim(SlugSeparator[0]);
        return From(slug.Length > MaxLength ? slug[..MaxLength].TrimEnd(SlugSeparator[0]) : slug);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();
}
