namespace RagWebScraper.Services;

using RagWebScraper.Models;

public class RagAnalysisWorker : BackgroundService
{
    private readonly IRagAnalysisQueue _queue;
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
    {
        _queue = queue;
        _logger = logger;
        _pageAnalyzer = pageAnalyzer;
        _chunkIngestor = chunkIngestor;
        _scraper = scraper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                var text = await _scraper.ScrapeTextAsync(request.Url);
                var analysis = await _pageAnalyzer.AnalyzePageAsync(request.Url, request.Keywords);
                if (analysis != null)
                    await _chunkIngestor.IngestChunksAsync(request.Url, text);
                request.Completion.SetResult(analysis);
                _logger.LogInformation("Processed {Url}", request.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process {Url}", request.Url);
                request.Completion.SetException(ex);
            }
        }
    }
}
