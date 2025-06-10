using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;


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
    public Task<IActionResult> Analyze([FromBody] UrlAnalysisRequest request) =>
        AnalyzeInternal(request);

    private async Task<IActionResult> AnalyzeInternal(UrlAnalysisRequest request)
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
