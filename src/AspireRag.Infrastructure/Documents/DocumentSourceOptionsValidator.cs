using Microsoft.Extensions.Options;

namespace AspireRag.Infrastructure.Documents;

internal sealed class DocumentSourceOptionsValidator : IValidateOptions<DocumentSourceOptions>
{
    public ValidateOptionsResult Validate(string? name, DocumentSourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Folders.Count == 0)
        {
            return ValidateOptionsResult.Fail($"Configure at least one folder in '{DocumentSourceOptions.SectionName}:Folders'.");
        }

        var failures = options.Folders
            .Where(static folder => string.IsNullOrWhiteSpace(folder.Name) || !Directory.Exists(folder.Path))
            .Select(static folder => $"Document folder '{folder.Name}' needs a name and an existing path, got '{folder.Path}'.")
            .ToList();

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
