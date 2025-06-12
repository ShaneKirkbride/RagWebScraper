using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class RagAnalyzerServiceTests
{
    private class StubLinker : ICrossDocumentLinker
    {
        public Task<IEnumerable<LinkedPassage>> LinkAsync(IEnumerable<DocumentChunk> chunks) => Task.FromResult<IEnumerable<LinkedPassage>>(new List<LinkedPassage>());
    }

    private class StubExtractor : IEntityGraphExtractor
    {
        public EntityGraph Extract(string text, string sourceId) => new EntityGraph(sourceId, [], []);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsEmpty_ForNoDocuments()
    {
        var service = new RagAnalyzerService(new StubLinker(), new StubExtractor());
        var result = await service.AnalyzeAsync(new DocumentSet([]));
        Assert.Empty(result.Links);
        Assert.Empty(result.EntityGraphs);
    }
}
