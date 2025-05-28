namespace RagWebScraper.Services
{
    public interface IEmbeddingService
    {
        Task<List<float>> GetEmbeddingAsync(string input);
        Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs);
    }
}