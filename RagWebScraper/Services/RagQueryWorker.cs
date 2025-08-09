using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Background worker that processes queued RAG queries.
/// </summary>
public class RagQueryWorker : ChannelBackgroundWorker<RagQueryRequest>
{
    private readonly ILogger<RagQueryWorker> _logger;
    private readonly IEmbeddingService _embedding;
    private readonly IVectorStoreService _vectorStore;
    private readonly IChatCompletionService _chat;

    public RagQueryWorker(
        IRagQueryQueue queue,
        ILogger<RagQueryWorker> logger,
        IEmbeddingService embedding,
        IVectorStoreService vectorStore,
        IChatCompletionService chat)
        : base(queue, logger)
    {
        _logger = logger;
        _embedding = embedding;
        _vectorStore = vectorStore;
        _chat = chat;
    }

    protected override async Task ProcessRequestAsync(RagQueryRequest request, CancellationToken stoppingToken)
    {
        var embedding = await _embedding.GetEmbeddingAsync(request.Query);
        var results = await _vectorStore.QueryAsync(embedding);

        var passages = results.Select(r => new RAGQueryResult
        {
            ChunkText = r.payload != null && r.payload.ContainsKey("ChunkText")
                ? r.payload["ChunkText"]?.ToString() ?? string.Empty
                : "[Missing ChunkText]",
            Source = r.payload != null && r.payload.ContainsKey("Source")
                ? r.payload["Source"]?.ToString()
                : null
        }).ToList();

        var commentary = string.Empty;
        if (passages.Count > 0)
        {
            try
            {
                var context = string.Join("\n---\n", passages.Select(p => p.ChunkText));
                var prompt = $"{context}\n\nProvide a brief commentary linking these passages.";
                var systemPrompt = "You are an assistant that offers insightful commentary on retrieved passages.";
                commentary = await _chat.GetCompletionAsync(systemPrompt, prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate commentary for query.");
            }
        }

        request.Completion.SetResult(new RAGQueryResponse
        {
            Results = passages,
            Commentary = commentary
        });
    }
}
