namespace RagWebScraper.Services;

/// <summary>
/// Service used to ingest chunks of text into the vector store.
/// </summary>
public interface IChunkIngestorService
{
    /// <summary>
    /// Splits and stores the provided text with associated metadata.
    /// </summary>
    /// <param name="sourceLabel">Label used to identify the text source.</param>
    /// <param name="text">The text to ingest.</param>
    /// <param name="extraMetadata">Optional additional metadata.</param>
    Task IngestChunksAsync(string sourceLabel, string text, Dictionary<string, object>? extraMetadata = null);
}

public class ChunkIngestorService : IChunkIngestorService
{
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStore;

    public ChunkIngestorService(TextChunker chunker, IEmbeddingService embedding, IVectorStoreService vectorStore)
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

                Console.WriteLine($"Inserted chunk from: {sourceLabel}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to insert chunk from {sourceLabel}: {ex.Message}");
            }
        }
    }
}
