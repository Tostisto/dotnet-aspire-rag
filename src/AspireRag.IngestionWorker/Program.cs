using AspireRag.Application;
using AspireRag.Application.UseCases.IngestDocuments;
using AspireRag.Infrastructure;
using AspireRag.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddKnowledgeBase();
builder.AddFileSystemDocumentSource();
builder.Services.AddIngestionUseCases();

using var host = builder.Build();
await host.StartAsync();

var stopping = host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
await host.Services.GetRequiredService<IngestDocumentsHandler>().HandleAsync(stopping);

await host.StopAsync();
