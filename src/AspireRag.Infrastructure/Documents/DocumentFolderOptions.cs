namespace AspireRag.Infrastructure.Documents;

public sealed class DocumentFolderOptions
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public Uri? BaseUrl { get; set; }
}
