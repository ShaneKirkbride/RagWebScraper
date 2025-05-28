using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Services;
using static RagWebScraper.Pages.UploadPdf;

[ApiController]
[Route("api/pdf")]
public class PdfUploadController : ControllerBase
{
    private readonly PdfTextExtractorService _extractor;
    private readonly SentimentAnalyzerService _sentiment;
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embedding;
    private readonly VectorStoreService _store;
    private readonly KeywordExtractorService _keywordExtractor;
    private readonly KeywordContextSentimentService _keywordContextSentimentService;

    private readonly IChunkIngestorService _chunkIngestor;

    public PdfUploadController(
        PdfTextExtractorService extractor,
        SentimentAnalyzerService sentiment,
        TextChunker chunker,
        IEmbeddingService embedding,
        VectorStoreService store,
        KeywordExtractorService keywordExtractor,
        KeywordContextSentimentService keywordContextSentimentService,
        IChunkIngestorService chunkIngestor)
    {
        _extractor = extractor;
        _sentiment = sentiment;
        _chunker = chunker;
        _embedding = embedding;
        _store = store;
        _keywordExtractor = keywordExtractor;
        _keywordContextSentimentService = keywordContextSentimentService;
        _chunkIngestor = chunkIngestor;
    }


    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzePdf([FromForm] IFormFileCollection files, [FromForm] string keywords)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var keywordList = keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ToList()
                          ?? new List<string>();

        var results = new List<object>();

        foreach (var file in files)
        {
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                continue;

            using var stream = file.OpenReadStream();
            var extractedText = _extractor.ExtractText(stream);
            var sentiment = _sentiment.AnalyzeSentiment(extractedText);
            var frequencies = _keywordExtractor.ExtractKeywords(extractedText, keywordList);
            var keywordSentiments = _keywordContextSentimentService.ExtractKeywordSentiments(extractedText, keywordList);

            await _chunkIngestor.IngestChunksAsync(file.FileName, extractedText, new Dictionary<string, object>
            {
                { "Sentiment", sentiment },
                { "SourceType", "PDF" }
            });

            results.Add(item: new FileSentimentSummary
            {
                FileName = file.FileName,
                Sentiment = sentiment,
                KeywordFrequencies = frequencies,
                KeywordSentiments = keywordSentiments,
                RawText = extractedText
            });
        }

        return Ok(results);
    }
}
