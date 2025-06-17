using System.Collections.Generic;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class CourtOpinionAnalyzerServiceTests
{
    private class StubSentiment : ISentimentAnalyzer
    {
        public float AnalyzeSentiment(string text) => 1.5f;
    }

    private class StubExtractor : IKeywordExtractor
    {
        public Dictionary<string, int> ExtractKeywords(string text, List<string> keywords) => keywords.ToDictionary(k => k, _ => 1);
    }

    private class StubContextService : IKeywordContextSentimentService
    {
        public Dictionary<string, float> ExtractKeywordSentiments(string text, IEnumerable<string> keywords) => keywords.ToDictionary(k => k, _ => 0.5f);
    }

    [Fact]
    public async Task AnalyzeOpinionAsync_ReturnsScores()
    {
        var service = new CourtOpinionAnalyzerService(new StubSentiment(), new StubExtractor(), new StubContextService());
        var opinion = new CourtOpinion { Id = 1, CaseName = "case", PlainText = "text" };
        var result = await service.AnalyzeOpinionAsync(opinion, new List<string> { "a" });

        Assert.Equal(1.5f, result.PageSentimentScore);
        Assert.Equal(1, result.KeywordFrequencies["a"]);
        Assert.Equal(0.5f, result.KeywordSentimentScores["a"]);
        Assert.Equal("text", result.RawText);
    }
}
