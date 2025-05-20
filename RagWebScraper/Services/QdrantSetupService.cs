namespace RagWebScraper.Services
{
    public class QdrantSetupService
    {
        private readonly HttpClient _http;
        private readonly string _collectionName = "rag";
        private readonly int _vectorSize = 1536;

        public QdrantSetupService(HttpClient http)
        {
            _http = http;
        }

        public async Task EnsureCollectionExistsAsync()
        {
            try
            {
                var response = await _http.GetAsync($"http://localhost:6333/collections/{_collectionName}");

                if (response.IsSuccessStatusCode)
                    return;

                var body = new
                {
                    vectors = new
                    {
                        size = _vectorSize,
                        distance = "Cosine"
                    }
                };

                var createResponse = await _http.PutAsJsonAsync(
                    $"http://localhost:6333/collections/{_collectionName}",
                    body);

                if (!createResponse.IsSuccessStatusCode)
                {
                    var error = await createResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to create collection: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Qdrant setup failed: {ex.Message}");
            }
        }
    }
}
