using AspireRag.Application.Abstractions;

namespace AspireRag.Api.Contracts;

public sealed record AskRequest(string? Question, RetrievalStrategy? Mode = null);
