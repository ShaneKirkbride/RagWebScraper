using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class RagResearchAgentTests
{
    private class StubEmbedding : IEmbeddingService
    {
        public Task<List<float>> GetEmbeddingAsync(string input) => Task.FromResult(new List<float> { 0.1f });
        public Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs) => Task.FromResult(new List<float[]> ());
    }

    private class StubVectorStore : IVectorStoreService
    {
        public List<QdrantResult> Results { get; init; } = new();
        public Task UpsertVectorAsync(VectorData vectorData) => Task.CompletedTask;
        public Task<List<QdrantResult>> QueryAsync(List<float> embedding, int topK = 3) => Task.FromResult(Results);
    }

    private class StubChat : IChatCompletionService
    {
        public string? Prompt { get; private set; }
        public string Response { get; init; } = "stub";
        public Task<string> GetCompletionAsync(string systemPrompt, string userPrompt)
        {
            Prompt = userPrompt;
            return Task.FromResult(Response);
        }
    }

    [Fact]
    public async Task ResearchAsync_ReturnsMessage_WhenNoResults()
    {
        var agent = new RagResearchAgent(new StubEmbedding(), new StubVectorStore(), new StubChat());
        var result = await agent.ResearchAsync("test");
        Assert.Equal("No relevant documents found.", result);
    }

    [Fact]
    public async Task ResearchAsync_UsesChatService()
    {
        var store = new StubVectorStore
        {
            Results = new List<QdrantResult>
            {
                new QdrantResult{ payload = new Dictionary<string, object>{{"ChunkText","A"}}, score = 0.9f }
            }
        };
        var chat = new StubChat { Response = "analysis" };
        var agent = new RagResearchAgent(new StubEmbedding(), store, chat);
        var result = await agent.ResearchAsync("test");
        Assert.Equal("analysis", result);
        Assert.Contains("A", chat.Prompt!);
    }
}
