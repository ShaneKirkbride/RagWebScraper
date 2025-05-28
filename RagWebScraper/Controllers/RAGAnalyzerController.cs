// Interface segregation and single responsibility principle applied
using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;

public interface IPageAnalyzerService
{
    Task<AnalysisResult> AnalyzePageAsync(string url, List<string> keywords);
}


// Service implementation for page analysis
public class PageAnalyzerService : IPageAnalyzerService
{
    private readonly IWebScraperService _scraper;
    private readonly SentimentAnalyzerService _sentiment;
    private readonly KeywordExtractorService _keywords;
    private readonly KeywordContextSentimentService _keywordContextSentimentService;

    public PageAnalyzerService(
        IWebScraperService scraper,
        SentimentAnalyzerService sentiment,
        KeywordExtractorService keywords,
        KeywordContextSentimentService keywordContextSentimentService)
    {
        _scraper = scraper;
        _sentiment = sentiment;
        _keywords = keywords;
        _keywordContextSentimentService = keywordContextSentimentService;
    }

    public async Task<AnalysisResult> AnalyzePageAsync(string url, List<string> keywords)
    {
        var text = await _scraper.ScrapeTextAsync(url);
        if (string.IsNullOrWhiteSpace(text)) return null;

        return new AnalysisResult(Enumerable.Empty<LinkedPassage>())
        {
            Url = url,
            PageSentimentScore = _sentiment.AnalyzeSentiment(text),
            KeywordFrequencies = _keywords.ExtractKeywords(text, keywords),
            KeywordSentimentScores = _keywordContextSentimentService.ExtractKeywordSentiments(text, keywords)
        };
    }
}

// Controller using SRP and DIP
[ApiController]
[Route("api/rag")]
public class RAGAnalyzerController : ControllerBase
{
    private readonly IPageAnalyzerService _pageAnalyzer;
    private readonly IChunkIngestorService _chunkIngestor;
    private readonly IWebScraperService _scraper;

    public RAGAnalyzerController(
        IPageAnalyzerService pageAnalyzer,
        IChunkIngestorService chunkIngestor,
        IWebScraperService scraper)
    {
        _pageAnalyzer = pageAnalyzer;
        _chunkIngestor = chunkIngestor;
        _scraper = scraper;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<List<AnalysisResult>>> Analyze([FromBody] UrlAnalysisRequest request)
    {
        var results = new List<AnalysisResult>();

        foreach (var url in request.Urls)
        {
            var text = await _scraper.ScrapeTextAsync(url);
            var analysis = await _pageAnalyzer.AnalyzePageAsync(url, request.Keywords);
            if (analysis != null)
            {
                results.Add(analysis);
                await _chunkIngestor.IngestChunksAsync(url, text);
            }
        }

        return Ok(results);
    }
}
