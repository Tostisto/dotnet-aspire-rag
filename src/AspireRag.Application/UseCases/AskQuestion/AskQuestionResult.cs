using AspireRag.Application.Abstractions;
using AspireRag.Domain.Conversations;

namespace AspireRag.Application.UseCases.AskQuestion;

public sealed record AskQuestionResult(Answer Answer, RetrievalStrategy Strategy, int SearchCount, TimeSpan Elapsed);
