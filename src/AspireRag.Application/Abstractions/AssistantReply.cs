using AspireRag.Domain.Conversations;

namespace AspireRag.Application.Abstractions;

public sealed record AssistantReply(Answer Answer, int SearchCount);
