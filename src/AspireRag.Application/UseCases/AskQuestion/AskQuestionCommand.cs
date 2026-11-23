using AspireRag.Application.Abstractions;

namespace AspireRag.Application.UseCases.AskQuestion;

public sealed record AskQuestionCommand(string? Question, RetrievalStrategy? Strategy = null);
