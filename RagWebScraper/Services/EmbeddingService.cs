using OpenAI.Embeddings;
using System.Linq;

namespace RagWebScraper.Services
{
    /// <summary>
    /// Provides helper methods for generating OpenAI embedding vectors.
    /// </summary>
    public class EmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _client;

        /// <summary>
        /// Initializes the service with the specified OpenAI API key.
        /// </summary>
        /// <param name="openAiKey">OpenAI API key used to authenticate requests.</param>
        public EmbeddingService(string openAiKey)
        {
            _client = new EmbeddingClient("text-embedding-3-small", openAiKey);
        }

        /// <summary>
        /// Generates an embedding vector for a single text input.
        /// </summary>
        /// <param name="input">The text to embed.</param>
        /// <returns>A list of floats representing the embedding.</returns>
        public async Task<List<float>> GetEmbeddingAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<float>();

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

        /// <summary>
        /// Generates embedding vectors for a collection of text inputs.
        /// </summary>
        /// <param name="inputs">The text inputs to embed.</param>
        /// <returns>A list of embedding vectors.</returns>
        public async Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs)
        {
            if (inputs == null || !inputs.Any())
                return new List<float[]>();

            OpenAIEmbeddingCollection result = await _client.GenerateEmbeddingsAsync(inputs);
            return result.Select(e => e.ToFloats().ToArray()).ToList();
        }
    }
}