using AspireRag.Application.UseCases.AskQuestion;
using AspireRag.Application.UseCases.IngestDocuments;
using AspireRag.Application.UseCases.SearchKnowledge;
using Microsoft.Extensions.DependencyInjection;

namespace AspireRag.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionAnsweringUseCases(this IServiceCollection services)
    {
        services.AddTransient<AskQuestionHandler>();
        services.AddTransient<SearchKnowledgeHandler>();
        return services;
    }

    public static IServiceCollection AddIngestionUseCases(this IServiceCollection services)
    {
        services.AddTransient<IngestDocumentsHandler>();
        return services;
    }
}
