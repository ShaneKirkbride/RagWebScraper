using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class KeywordContextSentimentServiceTests
{
    private class StubSentiment : ISentimentAnalyzer
    {
        public float AnalyzeSentiment(string text) => text.Contains("good") ? 1f : 0f;
    }

    [Fact]
    public void ExtractKeywordSentiments_ComputesAverageScores()
    {
        var service = new KeywordContextSentimentService(new StubSentiment());
        var text = "good apple. bad apple. good banana.";
        var keywords = new List<string> { "apple", "banana" };

        var result = service.ExtractKeywordSentiments(text, keywords);

        Assert.True(result["apple"] < result["banana"]);
        Assert.Equal(1f, result["banana"], 1);
    }

    [Fact]
    public void ExtractKeywordSentiments_IgnoresSubstringMatches()
    {
        var service = new KeywordContextSentimentService(new StubSentiment());
        var text = "The cart is slow. The art is good.";
        var keywords = new List<string> { "art" };

        var result = service.ExtractKeywordSentiments(text, keywords);

        Assert.Equal(1f, result["art"]);
    }
}
