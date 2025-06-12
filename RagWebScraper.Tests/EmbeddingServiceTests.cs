using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class EmbeddingServiceTests
{
    [Fact]
    public async Task GetEmbeddingsAsync_ReturnsEmpty_ForEmptyInput()
    {
        var service = new EmbeddingService("test-key");
        var result = await service.GetEmbeddingsAsync([]);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ReturnsEmpty_ForNullOrWhitespace()
    {
        var service = new EmbeddingService("test-key");
        var resultNull = await service.GetEmbeddingAsync(null!);
        var resultEmpty = await service.GetEmbeddingAsync(" ");
        Assert.Empty(resultNull);
        Assert.Empty(resultEmpty);
    }
}
