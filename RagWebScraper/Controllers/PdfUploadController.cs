using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;
using static RagWebScraper.Pages.UploadPdf;
using static RagWebScraper.Pages.KnowledgeGraph;

[ApiController]
[Route("api/pdf")]
public class PdfUploadController : ControllerBase
{
    private readonly ITextExtractor _extractor;
    private readonly ISentimentAnalyzer _sentiment;
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embedding;
    private readonly VectorStoreService _store;
    private readonly IKeywordExtractor _keywordExtractor;
    private readonly IKeywordContextSentimentService _keywordContextSentimentService;
    private readonly IChunkIngestorService _chunkIngestor;
    private readonly IPdfProcessingQueue _queue;

    public PdfUploadController(
        ITextExtractor extractor,
        ISentimentAnalyzer sentiment,
        TextChunker chunker,
        IEmbeddingService embedding,
        VectorStoreService store,
        IKeywordExtractor keywordExtractor,
        IKeywordContextSentimentService keywordContextSentimentService,
        IChunkIngestorService chunkIngestor,
        IPdfProcessingQueue queue)
    {
        _extractor = extractor;
        _sentiment = sentiment;
        _chunker = chunker;
        _embedding = embedding;
        _store = store;
        _keywordExtractor = keywordExtractor;
        _keywordContextSentimentService = keywordContextSentimentService;
        _chunkIngestor = chunkIngestor;
        _queue = queue;
    }

    [HttpPost("analyze")]
    public Task<IActionResult> AnalyzePdf([FromForm] IFormFileCollection files, [FromForm] string keywords) =>
        AnalyzePdfInternal(files, keywords);

    private async Task<IActionResult> AnalyzePdfInternal(IFormFileCollection files, string keywords)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var keywordList = keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)?.ToList()
                           ?? new List<string>();

        foreach (var file in files)
        {
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                continue;

            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            _queue.Enqueue(new PdfProcessingRequest
            {
                FileName = file.FileName,
                FileStream = memoryStream,
                Keywords = keywordList
            });
        }

        return Ok(new { Message = "PDFs are being processed in the background." });
    }

}
