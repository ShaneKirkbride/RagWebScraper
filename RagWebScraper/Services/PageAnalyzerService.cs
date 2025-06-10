namespace RagWebScraper.Services;

using RagWebScraper.Models;

public class PageAnalyzerService : IPageAnalyzerService
{
    private readonly IWebScraperService _scraper;
    private readonly ISentimentAnalyzer _sentiment;
    private readonly IKeywordExtractor _keywords;
    private readonly IKeywordContextSentimentService _keywordContextSentimentService;

    public PageAnalyzerService(
        IWebScraperService scraper,
        ISentimentAnalyzer sentiment,
        IKeywordExtractor keywords,
        IKeywordContextSentimentService keywordContextSentimentService)
    {
        _scraper = scraper;
        _sentiment = sentiment;
        _keywords = keywords;
        _keywordContextSentimentService = keywordContextSentimentService;
    }

    public async Task<AnalysisResult?> AnalyzePageAsync(string url, List<string> keywords)
    {
        var text = await _scraper.ScrapeTextAsync(url);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var sentences = SentenceSplitter.Split(text);
        return new AnalysisResult(Enumerable.Empty<LinkedPassage>())
        {
            Url = url,
            PageSentimentScore = _sentiment.AnalyzeSentiment(text),
            KeywordFrequencies = _keywords.ExtractKeywords(text, keywords),
            KeywordSentimentScores = _keywordContextSentimentService.ExtractKeywordSentiments(text, keywords),
            RawText = text,
            RawSentences = sentences
        };
    }
}
