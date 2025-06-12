using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class SemanticCrossLinkerTests
{
    private class StubEmbedding : IEmbeddingService
    {
        public Task<List<float>> GetEmbeddingAsync(string input) => Task.FromResult(new List<float>());
        public Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs) => Task.FromResult(new List<float[]>());
    }

    [Fact]
    public async Task LinkAsync_ReturnsEmpty_ForEmptyChunks()
    {
        var linker = new SemanticCrossLinker(new StubEmbedding());
        var result = await linker.LinkAsync([]);
        Assert.Empty(result);
    }

    [Fact]
    public async Task LinkAsync_ReturnsEmpty_ForSingleChunk()
    {
        var linker = new SemanticCrossLinker(new StubEmbedding());
        var result = await linker.LinkAsync(new[] { new DocumentChunk("a", "text") });
        Assert.Empty(result);
    }
}
