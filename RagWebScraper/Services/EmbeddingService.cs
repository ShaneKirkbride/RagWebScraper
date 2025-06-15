using OpenAI.Embeddings;
using Polly;
using Polly.Retry;
using System.Linq;
using System.Net;
using System.ClientModel;

namespace RagWebScraper.Services
{
    /// <summary>
    /// Provides helper methods for generating OpenAI embedding vectors.
    /// </summary>
    public class EmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _client;
        private readonly AsyncRetryPolicy<ClientResult<OpenAIEmbeddingCollection>> _retryPolicy;

        /// <summary>
        /// Initializes the service with the specified OpenAI API key.
        /// </summary>
        /// <param name="openAiKey">OpenAI API key used to authenticate requests.</param>
        public EmbeddingService(string openAiKey)
        {
            _client = new EmbeddingClient("text-embedding-3-small", openAiKey);
            _retryPolicy = Policy<ClientResult<OpenAIEmbeddingCollection>>
                .Handle<ClientResultException>(ex => ex.Status == (int)HttpStatusCode.TooManyRequests)
                .Or<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, timespan, attempt, context) =>
                    {
                        Console.WriteLine($"[Retry {attempt}] Rate limit hit. Waiting {timespan} before next try.");
                    });
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
            ClientResult<OpenAIEmbeddingCollection> result = await _retryPolicy.ExecuteAsync(() => _client.GenerateEmbeddingsAsync([input]));
            OpenAIEmbeddingCollection collection = result.Value;

            foreach (OpenAIEmbedding embedding in collection)
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

            ClientResult<OpenAIEmbeddingCollection> resultMulti = await _retryPolicy.ExecuteAsync(() => _client.GenerateEmbeddingsAsync(inputs));
            OpenAIEmbeddingCollection collectionMulti = resultMulti.Value;
            return collectionMulti.Select(e => e.ToFloats().ToArray()).ToList();
        }
    }
}