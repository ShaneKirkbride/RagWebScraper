// Interface segregation and single responsibility principle applied
using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;

public interface IPageAnalyzerService
{
    Task<AnalysisResult> AnalyzePageAsync(string url, List<string> keywords);
}

public interface IChunkIngestorService
{
    Task IngestChunksAsync(string sourceLabel, string text, Dictionary<string, object>? extraMetadata = null);
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

        return new AnalysisResult
        {
            Url = url,
            PageSentimentScore = _sentiment.AnalyzeSentiment(text),
            KeywordFrequencies = _keywords.ExtractKeywords(text, keywords),
            KeywordSentimentScores = _keywordContextSentimentService.ExtractKeywordSentiments(text, keywords)
        };
    }
}

public class ChunkIngestorService : IChunkIngestorService
{
    private readonly TextChunker _chunker;
    private readonly EmbeddingService _embedding;
    private readonly VectorStoreService _vectorStore;

    public ChunkIngestorService(TextChunker chunker, EmbeddingService embedding, VectorStoreService vectorStore)
    {
        _chunker = chunker;
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    public async Task IngestChunksAsync(string sourceLabel, string text, Dictionary<string, object>? extraMetadata = null)
    {
        var chunks = _chunker.ChunkText(text);

        foreach (var chunk in chunks)
        {
            try
            {
                var embedding = await _embedding.GetEmbeddingAsync(chunk);

                var metadata = new Dictionary<string, object>
                {
                    { "ChunkText", chunk },
                    { "Source", sourceLabel }
                };

                if (extraMetadata != null)
                {
                    foreach (var kvp in extraMetadata)
                        metadata[kvp.Key] = kvp.Value;
                }

                await _vectorStore.UpsertVectorAsync(new VectorData
                {
                    Id = Guid.NewGuid().ToString(),
                    Embedding = embedding,
                    Metadata = metadata
                });

                Console.WriteLine($"✅ Inserted chunk from: {sourceLabel}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to insert chunk from {sourceLabel}: {ex.Message}");
            }
        }
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
