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
    private readonly IRagAnalysisQueue _queue;

    public RAGAnalyzerController(IRagAnalysisQueue queue)
    {
        _queue = queue;
    }

    [HttpPost("analyze")]
    public Task<IActionResult> Analyze([FromBody] UrlAnalysisRequest request) =>
        AnalyzeInternal(request);

    private async Task<IActionResult> AnalyzeInternal(UrlAnalysisRequest request)
    {
        var tasks = new List<Task<AnalysisResult?>>();

        foreach (var url in request.Urls)
        {
            var queueRequest = new RagAnalysisRequest
            {
                Url = url,
                Keywords = request.Keywords
            };

            _queue.Enqueue(queueRequest);
            tasks.Add(queueRequest.Completion.Task);
        }

        var results = (await Task.WhenAll(tasks)).Where(r => r != null).ToList()!;
        return Ok(results);
    }

}
