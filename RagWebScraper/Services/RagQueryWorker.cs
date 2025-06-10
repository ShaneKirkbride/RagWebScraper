namespace RagWebScraper.Services;

public class RagQueryWorker : BackgroundService
{
    private readonly IRagQueryQueue _queue;
    private readonly ILogger<RagQueryWorker> _logger;
    private readonly IEmbeddingService _embedding;
    private readonly VectorStoreService _vectorStore;

    public RagQueryWorker(
        IRagQueryQueue queue,
        ILogger<RagQueryWorker> logger,
        IEmbeddingService embedding,
        VectorStoreService vectorStore)
    {
        _queue = queue;
        _logger = logger;
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                var embedding = await _embedding.GetEmbeddingAsync(request.Query);
                var results = await _vectorStore.QueryAsync(embedding);
                var texts = results.Select(r =>
                    r.payload != null && r.payload.ContainsKey("ChunkText")
                        ? r.payload["ChunkText"]?.ToString()
                        : "[Missing ChunkText]").ToList();
                request.Completion.SetResult(texts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process query");
                request.Completion.SetException(ex);
            }
        }
    }
}
