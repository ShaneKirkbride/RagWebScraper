namespace RagWebScraper.Services;

using RagWebScraper.Models;

public class RagAnalysisWorker : ChannelBackgroundWorker<RagAnalysisRequest>
{
    private readonly ILogger<RagAnalysisWorker> _logger;
    private readonly IPageAnalyzerService _pageAnalyzer;
    private readonly IChunkIngestorService _chunkIngestor;
    private readonly IWebScraperService _scraper;

    public RagAnalysisWorker(
        IRagAnalysisQueue queue,
        ILogger<RagAnalysisWorker> logger,
        IPageAnalyzerService pageAnalyzer,
        IChunkIngestorService chunkIngestor,
        IWebScraperService scraper)
        : base(queue, logger)
    {
        _logger = logger;
        _pageAnalyzer = pageAnalyzer;
        _chunkIngestor = chunkIngestor;
        _scraper = scraper;
    }

    protected override async Task ProcessRequestAsync(RagAnalysisRequest request, CancellationToken stoppingToken)
    {
        var text = await _scraper.ScrapeTextAsync(request.Url);
        var analysis = await _pageAnalyzer.AnalyzePageAsync(request.Url, request.Keywords);
        if (analysis != null)
            await _chunkIngestor.IngestChunksAsync(request.Url, text);
        request.Completion.SetResult(analysis);
        _logger.LogInformation("Processed {Url}", request.Url);
    }
}
