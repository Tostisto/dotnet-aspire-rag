using System.Text.Json.Serialization;
using AspireRag.Api.Endpoints;
using AspireRag.Api.Errors;
using AspireRag.Application;
using AspireRag.Infrastructure;
using AspireRag.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKnowledgeBase();
builder.AddAssistant();
builder.Services.AddQuestionAnsweringUseCases();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
app.MapKnowledgeEndpoints();

app.Run();
