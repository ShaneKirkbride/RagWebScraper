using OpenAI.Embeddings;
using System.Linq;

namespace RagWebScraper.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _client;

        public EmbeddingService(string openAiKey)
        {
            _client = new EmbeddingClient("text-embedding-3-small", openAiKey);
        }

        public async Task<List<float>> GetEmbeddingAsync(string input)
        {

            List<float> embeddingsVectors = new List<float>();
            OpenAIEmbeddingCollection? result = await _client.GenerateEmbeddingsAsync([input]);

            foreach (OpenAIEmbedding embedding in result)
            {
                ReadOnlyMemory<float> vector = embedding.ToFloats();

                Console.WriteLine($"Dimension: {vector.Length}");
                Console.WriteLine($"Floats: ");
                for (int i = 0; i < vector.Length; i++)
                {
                    Console.WriteLine($"  [{i,4}] = {vector.Span[i]}");
                }

                Console.WriteLine();
                // Extract ReadOnlyMemory<float> and append to your list
                embeddingsVectors.AddRange(embedding.ToFloats().ToArray());
            }

            return embeddingsVectors;
        }
        public async Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs)
        {
            OpenAIEmbeddingCollection result = await _client.GenerateEmbeddingsAsync(inputs);
            return result.Select(e => e.ToFloats().ToArray()).ToList();
        }
    }
}