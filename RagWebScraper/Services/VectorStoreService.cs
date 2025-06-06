using System.Net.Http.Json;

namespace RagWebScraper.Services
{
    public class VectorStoreService
    {
        private readonly HttpClient _httpClient;
        private readonly string _collectionName = "rag";
        private readonly string _qdrantBaseUrl = "http://localhost:6333";

        public VectorStoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpsertVectorAsync(VectorData vectorData)
        {
            var payload = new
            {
                points = new[]
                {
            new
            {
                id = vectorData.Id,
                vector = vectorData.Embedding,
                payload = vectorData.Metadata
            }
        }
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"http://localhost:6333/collections/rag/points", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Qdrant insert failed: {response.StatusCode} - {error}");
            }
            else
            {
                Console.WriteLine($"Vector inserted: {vectorData.Id}");
            }
        }

        public async Task<List<QdrantResult>> QueryAsync(List<float> embedding, int topK = 3)
        {
            var body = new
            {
                vector = embedding,
                top = topK,
                with_payload = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_qdrantBaseUrl}/collections/{_collectionName}/points/search",
                body);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Qdrant search failed: {response.StatusCode}");
                return new List<QdrantResult>();
            }

            var result = await response.Content.ReadFromJsonAsync<QdrantQueryResponse>();
            return result?.result ?? new List<QdrantResult>();
        }
    }

    public class VectorData
    {
        public string Id { get; set; }
        public List<float> Embedding { get; set; }
        public object Metadata { get; set; }
    }

    public class QdrantQueryResponse
    {
        public List<QdrantResult> result { get; set; }
    }

    public class QdrantResult
    {
        public float score { get; set; }
        public Dictionary<string, object> payload { get; set; }
    }
}
