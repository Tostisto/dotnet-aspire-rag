using System.Runtime.CompilerServices;
using AspireRag.Application.Abstractions;
using AspireRag.Domain.Documents;
using Microsoft.Extensions.Options;

namespace AspireRag.Infrastructure.Documents;

internal sealed class FileSystemDocumentSource(IOptions<DocumentSourceOptions> options) : IDocumentSource
{
    private const string SearchPattern = "*.md";
    private const char WordSeparator = '-';
    private const char Space = ' ';

    public async IAsyncEnumerable<SourceDocument> GetDocumentsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var folder in options.Value.Folders)
        {
            var root = Path.GetFullPath(folder.Path);
            foreach (var file in Directory.EnumerateFiles(root, SearchPattern, SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                var markdown = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
                var content = MarkdownCleaner.Clean(markdown);
                if (content.Length == 0)
                {
                    continue;
                }

                var page = Path.ChangeExtension(Path.GetRelativePath(root, file), null).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var title = MarkdownCleaner.ReadTitle(markdown) ?? Path.GetFileName(page).Replace(WordSeparator, Space);
                var url = folder.BaseUrl is null ? null : new Uri(folder.BaseUrl, page);

                yield return new SourceDocument(DocumentId.FromName($"{folder.Name}/{page}"), title, content, url);
            }
        }
    }
}
