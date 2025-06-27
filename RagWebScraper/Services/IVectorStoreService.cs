namespace RagWebScraper.Services;

/// <summary>
/// Abstraction over the vector database.
/// </summary>
public interface IVectorStoreService
{
    Task UpsertVectorAsync(VectorData vectorData);
    Task<List<QdrantResult>> QueryAsync(List<float> embedding, int topK = 3);
}
