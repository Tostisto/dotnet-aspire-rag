using AspireRag.Application;
using AspireRag.Application.Abstractions;
using AspireRag.Infrastructure.Assistant;
using AspireRag.Infrastructure.Documents;
using AspireRag.Infrastructure.Knowledge;
using CommunityToolkit.VectorData.Qdrant;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;

namespace AspireRag.Infrastructure;

public static class InfrastructureHostBuilderExtensions
{
    public static IHostApplicationBuilder AddKnowledgeBase(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddRagOptions();
        builder.Services.AddOptions<KnowledgeBaseOptions>()
            .BindConfiguration(KnowledgeBaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.AddQdrantClient(ResourceNames.Qdrant);
        builder.AddKeyedOllamaApiClient(ResourceNames.EmbeddingModel).AddEmbeddingGenerator();

        builder.Services.AddSingleton<VectorStoreCollection<Guid, ChunkRecord>>(static services =>
        {
            var options = services.GetRequiredService<IOptions<KnowledgeBaseOptions>>().Value;
            return new QdrantCollection<Guid, ChunkRecord>(
                services.GetRequiredService<QdrantClient>(),
                options.CollectionName,
                ownsClient: false,
                new QdrantCollectionOptions
                {
                    Definition = ChunkRecord.CreateDefinition(options.EmbeddingDimensions),
                    EmbeddingGenerator = services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(),
                });
        });
        builder.Services.AddSingleton(static services =>
            new MarkdownChunker(services.GetRequiredService<IOptions<KnowledgeBaseOptions>>().Value.MaxChunkCharacters));
        builder.Services.AddSingleton<IKnowledgeBase, VectorStoreKnowledgeBase>();

        return builder;
    }

    public static IHostApplicationBuilder AddAssistant(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddRagOptions();

        var captureMessageContent = builder.Environment.IsDevelopment();
        builder.AddKeyedOllamaApiClient(ResourceNames.ChatModel)
            .AddChatClient(telemetry => telemetry.EnableSensitiveData = captureMessageContent);
        builder.Services.AddSingleton<IAssistant, AgentAssistant>();

        return builder;
    }

    public static IHostApplicationBuilder AddFileSystemDocumentSource(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<DocumentSourceOptions>()
            .BindConfiguration(DocumentSourceOptions.SectionName)
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<DocumentSourceOptions>, DocumentSourceOptionsValidator>();
        builder.Services.AddSingleton<IDocumentSource, FileSystemDocumentSource>();

        return builder;
    }

    private static void AddRagOptions(this IHostApplicationBuilder builder) =>
        builder.Services.AddOptions<RagOptions>()
            .BindConfiguration(RagOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
}
