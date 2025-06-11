using System.Collections.Generic;
using System.Linq;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class OnnxEntityExtractorTests
{
    private class StubNerService : INerService
    {
        public List<NamedEntity> RecognizeEntities(string text) =>
            new() { new NamedEntity { Text = "Microsoft", Label = "ORG", Start = 0, End = 1 } };

        public List<(string Token, string Label)> RecognizeTokensWithLabels(string sentence) =>
            new();
    }

    [Fact]
    public void ExtractEntities_ReturnsMappedEntities()
    {
        IEntityExtractor extractor = new OnnxEntityExtractor(new StubNerService());
        var result = extractor.ExtractEntities("test").ToList();

        Assert.Single(result);
        var entity = result[0];
        Assert.Equal("ORG", entity.EntityType);
        Assert.Equal("Microsoft", entity.EntityText);
        Assert.Equal(0, entity.StartIndex);
        Assert.Equal(1, entity.EndIndex);
    }
}
