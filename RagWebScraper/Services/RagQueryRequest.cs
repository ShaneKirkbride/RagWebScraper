using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Request object used for queued RAG queries.
/// </summary>
public class RagQueryRequest
{
    public required string Query { get; init; }
    public TaskCompletionSource<RAGQueryResponse> Completion { get; } = new();
}
