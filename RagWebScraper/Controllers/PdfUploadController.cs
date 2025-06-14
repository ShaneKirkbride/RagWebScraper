using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using RagWebScraper.Models;
using RagWebScraper.Services;
using static RagWebScraper.Pages.UploadPdf;
using static RagWebScraper.Pages.KnowledgeGraph;

[ApiController]
[Route("api/pdf")]
/// <summary>
/// API controller that processes PDF uploads and enqueues analysis tasks.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfUploadController"/> class.
    /// </summary>
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

    /// <summary>
    /// Endpoint to enqueue PDFs for asynchronous analysis.
    /// </summary>
    [HttpPost("analyze")]
    public Task<IActionResult> AnalyzePdf([FromForm] IFormFileCollection files, [FromForm] string keywords) =>
        AnalyzePdfInternal(files, keywords);

    /// <summary>
    /// Performs the actual PDF processing logic and enqueues background jobs.
    /// </summary>
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

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}");
            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            _queue.Enqueue(new PdfProcessingRequest
            {
                FileName = file.FileName,
                FilePath = tempPath,
                Keywords = keywordList
            });
        }

        return Ok(new { Message = "PDFs are being processed in the background." });
    }

}
