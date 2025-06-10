namespace RagWebScraper.Services;

public class RagQueryWorker : ChannelBackgroundWorker<RagQueryRequest>
{
    private readonly ILogger<RagQueryWorker> _logger;
    private readonly IEmbeddingService _embedding;
    private readonly VectorStoreService _vectorStore;

    public RagQueryWorker(
        IRagQueryQueue queue,
        ILogger<RagQueryWorker> logger,
        IEmbeddingService embedding,
        VectorStoreService vectorStore)
        : base(queue, logger)
    {
        _logger = logger;
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    protected override async Task ProcessRequestAsync(RagQueryRequest request, CancellationToken stoppingToken)
    {
        var embedding = await _embedding.GetEmbeddingAsync(request.Query);
        var results = await _vectorStore.QueryAsync(embedding);
        var texts = results.Select(r =>
            r.payload != null && r.payload.ContainsKey("ChunkText")
                ? r.payload["ChunkText"]?.ToString()
                : "[Missing ChunkText]").ToList();
        request.Completion.SetResult(texts);
    }
}
