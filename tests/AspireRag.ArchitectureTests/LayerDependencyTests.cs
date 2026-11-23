using System.Reflection;
using AspireRag.Infrastructure;
using NetArchTest.Rules;

namespace AspireRag.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(Domain.Documents.SourceDocument).Assembly;
    private static readonly Assembly Application = typeof(Application.RagOptions).Assembly;
    private static readonly Assembly Infrastructure = typeof(InfrastructureHostBuilderExtensions).Assembly;
    private static readonly Assembly Api = typeof(Api.Contracts.AskRequest).Assembly;

    private static readonly string[] AiAndVendorNamespaces =
    [
        "Microsoft.Extensions.AI", "Microsoft.Agents", "Microsoft.Extensions.VectorData",
        "Qdrant", "CommunityToolkit", "OllamaSharp",
    ];

    private static readonly string[] InfrastructureInternals =
    [
        "AspireRag.Infrastructure.Assistant", "AspireRag.Infrastructure.Documents", "AspireRag.Infrastructure.Knowledge",
    ];

    [Fact]
    public void Domain_depends_on_nothing_else_in_the_solution() =>
        Types.InAssembly(Domain).ShouldNot()
            .HaveDependencyOnAny("AspireRag.Application", "AspireRag.Infrastructure", "AspireRag.Api", "Microsoft.Extensions")
            .GetResult().ShouldBeSuccessful();

    [Fact]
    public void Application_does_not_depend_on_Infrastructure_or_hosts() =>
        Types.InAssembly(Application).ShouldNot()
            .HaveDependencyOnAny("AspireRag.Infrastructure", "AspireRag.Api", "AspireRag.IngestionWorker", "Microsoft.AspNetCore")
            .GetResult().ShouldBeSuccessful();

    [Fact]
    public void Application_has_no_AI_or_vendor_dependencies() =>
        Types.InAssembly(Application).ShouldNot()
            .HaveDependencyOnAny(AiAndVendorNamespaces)
            .GetResult().ShouldBeSuccessful();

    [Fact]
    public void Infrastructure_does_not_depend_on_hosts() =>
        Types.InAssembly(Infrastructure).ShouldNot()
            .HaveDependencyOnAny("AspireRag.Api", "AspireRag.IngestionWorker", "Microsoft.AspNetCore")
            .GetResult().ShouldBeSuccessful();

    [Fact]
    public void Api_uses_only_the_infrastructure_registration_never_its_adapters_or_vendor_types() =>
        Types.InAssembly(Api).ShouldNot()
            .HaveDependencyOnAny([.. AiAndVendorNamespaces, .. InfrastructureInternals])
            .GetResult().ShouldBeSuccessful();

    [Fact]
    public void Infrastructure_adapters_are_internal() =>
        Types.InAssembly(Infrastructure)
            .That().ImplementInterface(typeof(Application.Abstractions.IKnowledgeBase))
            .Or().ImplementInterface(typeof(Application.Abstractions.IAssistant))
            .Or().ImplementInterface(typeof(Application.Abstractions.IDocumentSource))
            .Should().NotBePublic()
            .GetResult().ShouldBeSuccessful();
}
