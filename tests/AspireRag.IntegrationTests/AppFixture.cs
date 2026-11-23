using System.Data.Common;
using Microsoft.Extensions.AI;
using OllamaSharp;

[assembly: AssemblyFixture(typeof(AspireRag.IntegrationTests.AppFixture))]

namespace AspireRag.IntegrationTests;

public sealed class AppFixture : IAsyncLifetime
{
    private const string ApiResource = "api";
    private const string ApiEndpoint = "http";
    private const string ChatResource = "chat";
    private const string EndpointKey = "Endpoint";
    private const string ModelKey = "Model";
    private const string NotStartedMessage = "The application has not been started.";

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan TotalRequestTimeout = TimeSpan.FromMinutes(6);
    private static readonly TimeSpan CircuitBreakerSamplingDuration = TimeSpan.FromMinutes(7);

    private DistributedApplication? _app;
    private HttpClient? _api;
    private OllamaApiClient? _chatModel;

    public HttpClient Api => _api ?? throw new InvalidOperationException(NotStartedMessage);

    public IChatClient ChatModel => _chatModel ?? throw new InvalidOperationException(NotStartedMessage);

    public async ValueTask InitializeAsync()
    {
        using var cts = new CancellationTokenSource(StartupTimeout);

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireRag_AppHost>(cts.Token);
        builder.Services.ConfigureHttpClientDefaults(static http => http.AddStandardResilienceHandler(static options =>
        {
            options.AttemptTimeout.Timeout = AttemptTimeout;
            options.TotalRequestTimeout.Timeout = TotalRequestTimeout;
            options.CircuitBreaker.SamplingDuration = CircuitBreakerSamplingDuration;
            options.Retry.MaxRetryAttempts = 1;
        }));

        _app = await builder.BuildAsync(cts.Token);
        await _app.StartAsync(cts.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(ApiResource, cts.Token);

        _api = _app.CreateHttpClient(ApiResource, ApiEndpoint);

        var connection = new DbConnectionStringBuilder { ConnectionString = await _app.GetConnectionStringAsync(ChatResource, cts.Token) };
        _chatModel = new OllamaApiClient(new Uri((string)connection[EndpointKey]), (string)connection[ModelKey]);
    }

    public async ValueTask DisposeAsync()
    {
        _api?.Dispose();
        _chatModel?.Dispose();
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
