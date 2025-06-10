namespace RagWebScraper.Services;

/// <summary>
/// Generates embeddings for text input.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Gets an embedding vector for a single text input.
    /// </summary>
    /// <param name="input">The text to embed.</param>
    Task<List<float>> GetEmbeddingAsync(string input);

    /// <summary>
    /// Gets embedding vectors for multiple texts.
    /// </summary>
    /// <param name="inputs">The texts to embed.</param>
    Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs);
}
