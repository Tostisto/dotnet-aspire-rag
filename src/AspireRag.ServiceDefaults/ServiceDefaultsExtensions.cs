using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AspireRag.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string LiveTag = "live";
    private const string SelfCheckName = "self";
    private const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan TotalRequestTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CircuitBreakerSamplingDuration = TimeSpan.FromMinutes(4);

    private static readonly string[] TelemetrySources =
        ["AspireRag.*", "Experimental.Microsoft.Extensions.AI", "Experimental.Microsoft.Agents.AI"];

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        ConfigureOpenTelemetry(builder);

        builder.Services.AddHealthChecks()
            .AddCheck(SelfCheckName, static () => HealthCheckResult.Healthy(), [LiveTag]);

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(static http =>
        {
            http.AddStandardResilienceHandler(static options =>
            {
                options.AttemptTimeout.Timeout = AttemptTimeout;
                options.TotalRequestTimeout.Timeout = TotalRequestTimeout;
                options.CircuitBreaker.SamplingDuration = CircuitBreakerSamplingDuration;
            });
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = static check => check.Tags.Contains(LiveTag),
            });
        }

        return app;
    }

    private static void ConfigureOpenTelemetry(IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(static logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .WithMetrics(static metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(TelemetrySources))
            .WithTracing(static tracing => tracing
                .AddSource(TelemetrySources)
                .AddAspNetCoreInstrumentation(static options => options.Filter = static context =>
                    !context.Request.Path.StartsWithSegments(HealthEndpointPath, StringComparison.OrdinalIgnoreCase)
                    && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath, StringComparison.OrdinalIgnoreCase))
                .AddHttpClientInstrumentation());

        if (!string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointKey]))
        {
            openTelemetry.UseOtlpExporter();
        }
    }
}
