using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class KeywordExtractorServiceTests
{
    [Fact]
    public void ExtractKeywords_CountsFrequencies()
    {
        IKeywordExtractor extractor = new KeywordExtractorService();
        var text = "Hello world. World says hello.";
        var result = extractor.ExtractKeywords(text, new List<string> { "hello", "world" });

        Assert.Equal(2, result["hello"]);
        Assert.Equal(2, result["world"]);
    }
}
