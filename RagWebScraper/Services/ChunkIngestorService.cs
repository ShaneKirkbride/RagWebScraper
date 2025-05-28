namespace RagWebScraper.Services;

public interface IChunkIngestorService
{
    Task IngestChunksAsync(string sourceLabel, string text, Dictionary<string, object>? extraMetadata = null);
}

public class ChunkIngestorService : IChunkIngestorService
{
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly VectorStoreService _vectorStore;

    public ChunkIngestorService(TextChunker chunker, IEmbeddingService embedding, VectorStoreService vectorStore)
    {
        _chunker = chunker;
        _embeddingService = embedding;
        _vectorStore = vectorStore;
    }

    public async Task IngestChunksAsync(string sourceLabel, string text, Dictionary<string, object>? extraMetadata = null)
    {
        var chunks = _chunker.ChunkText(text);

        foreach (var chunk in chunks)
        {
            try
            {
                var embedding = await _embeddingService.GetEmbeddingAsync(chunk);

                var metadata = new Dictionary<string, object>
            {
                { "ChunkText", chunk },
                { "Source", sourceLabel }
            };

                if (extraMetadata != null)
                {
                    foreach (var kvp in extraMetadata)
                        metadata[kvp.Key] = kvp.Value;
                }

                await _vectorStore.UpsertVectorAsync(new VectorData
                {
                    Id = Guid.NewGuid().ToString(),
                    Embedding = embedding,
                    Metadata = metadata
                });

                Console.WriteLine($"✅ Inserted chunk from: {sourceLabel}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to insert chunk from {sourceLabel}: {ex.Message}");
            }
        }
    }
}