using Microsoft.Extensions.Configuration;

const string QdrantResource = "qdrant";
const string OllamaResource = "ollama";
const string ChatResource = "chat";
const string EmbeddingsResource = "embeddings";
const string IngestionResource = "ingestion";
const string ApiResource = "api";
const string IngestionProject = "../AspireRag.IngestionWorker/AspireRag.IngestionWorker.csproj";
const string ApiProject = "../AspireRag.Api/AspireRag.Api.csproj";
const int LocalOllamaPort = 11435;

const string UseLocalOllamaKey = "Ollama:UseLocal";
const string ChatModelKey = "AI:ChatModel";
const string EmbeddingModelKey = "AI:EmbeddingModel";
const string EmbeddingDimensionsKey = "AI:EmbeddingDimensions";
const string DocumentEmbeddingPrefixKey = "AI:DocumentEmbeddingPrefix";
const string QueryEmbeddingPrefixKey = "AI:QueryEmbeddingPrefix";
const string DocumentsSection = "Documents";
const string PathKeySuffix = ":Path";
const string ConfigKeySeparator = ":";
const string EnvironmentKeySeparator = "__";

const string EmbeddingDimensionsVariable = "KnowledgeBase__EmbeddingDimensions";
const string DocumentEmbeddingPrefixVariable = "KnowledgeBase__DocumentEmbeddingPrefix";
const string QueryEmbeddingPrefixVariable = "KnowledgeBase__QueryEmbeddingPrefix";

const string HttpEndpoint = "http";
const string ScalarPath = "/scalar";
const string ScalarDisplayText = "Scalar";

var builder = DistributedApplication.CreateBuilder(args);
var config = builder.Configuration;

var qdrant = builder.AddQdrant(QdrantResource)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<IOllamaResource> ollama = config.GetValue(UseLocalOllamaKey, defaultValue: true)
    ? builder.AddOllamaLocal(OllamaResource, targetPort: LocalOllamaPort)
    : builder.AddOllama(OllamaResource).WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var chat = ollama.AddModel(ChatResource, config[ChatModelKey]!);
var embeddings = ollama.AddModel(EmbeddingsResource, config[EmbeddingModelKey]!);

var ingestion = builder.AddDotnetProject(IngestionResource, IngestionProject)
    .WithReference(qdrant).WaitFor(qdrant)
    .WithReference(embeddings).WaitFor(embeddings)
    .WithEnvironment(EmbeddingDimensionsVariable, config[EmbeddingDimensionsKey])
    .WithEnvironment(DocumentEmbeddingPrefixVariable, config[DocumentEmbeddingPrefixKey]);

foreach (var (key, value) in config.GetSection(DocumentsSection).AsEnumerable(makePathsRelative: true))
{
    if (value is not null)
    {
        var resolved = key.EndsWith(PathKeySuffix, StringComparison.Ordinal) ? Path.GetFullPath(value, builder.AppHostDirectory) : value;
        var variable = $"{DocumentsSection}{EnvironmentKeySeparator}{key.Replace(ConfigKeySeparator, EnvironmentKeySeparator, StringComparison.Ordinal)}";
        ingestion.WithEnvironment(variable, resolved);
    }
}

builder.AddDotnetProject(ApiResource, ApiProject)
    .WithReference(qdrant).WaitFor(qdrant)
    .WithReference(embeddings).WaitFor(embeddings)
    .WithReference(chat).WaitFor(chat)
    .WaitForCompletion(ingestion)
    .WithEnvironment(EmbeddingDimensionsVariable, config[EmbeddingDimensionsKey])
    .WithEnvironment(QueryEmbeddingPrefixVariable, config[QueryEmbeddingPrefixKey])
    .WithUrlForEndpoint(HttpEndpoint, static _ => new() { Url = ScalarPath, DisplayText = ScalarDisplayText })
    .WithExternalHttpEndpoints();

builder.Build().Run();
