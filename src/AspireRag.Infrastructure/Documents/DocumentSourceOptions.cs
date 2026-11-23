namespace AspireRag.Infrastructure.Documents;

public sealed class DocumentSourceOptions
{
    public const string SectionName = "Documents";

    public IList<DocumentFolderOptions> Folders { get; set; } = [];
}
