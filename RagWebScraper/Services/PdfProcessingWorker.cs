namespace RagWebScraper.Services;

using RagWebScraper.Models;

public class PdfProcessingWorker : ChannelBackgroundWorker<PdfProcessingRequest>
{
    private readonly ILogger<PdfProcessingWorker> _logger;
    private readonly ITextExtractor _extractor;
    private readonly ISentimentAnalyzer _sentiment;
    private readonly IKeywordExtractor _keywordExtractor;
    private readonly IKeywordContextSentimentService _contextSentiment;
    private readonly IChunkIngestorService _chunkIngestor;
    private readonly PdfResultStore _resultStore; // ✅ Injected result store

    public PdfProcessingWorker(
        IPdfProcessingQueue queue,
        ILogger<PdfProcessingWorker> logger,
        ITextExtractor extractor,
        ISentimentAnalyzer sentiment,
        IKeywordExtractor keywordExtractor,
        IKeywordContextSentimentService contextSentiment,
        IChunkIngestorService chunkIngestor,
        PdfResultStore resultStore) // ✅ Inject constructor dependency
        : base(queue, logger)
    {
        _logger = logger;
        _extractor = extractor;
        _sentiment = sentiment;
        _keywordExtractor = keywordExtractor;
        _contextSentiment = contextSentiment;
        _chunkIngestor = chunkIngestor;
        _resultStore = resultStore;
    }

    protected override async Task ProcessRequestAsync(PdfProcessingRequest request, CancellationToken stoppingToken)
    {
        var text = _extractor.ExtractText(request.FileStream);
        var sentences = SentenceSplitter.Split(text);

        var sentiment = _sentiment.AnalyzeSentiment(text);
        var keywords = _keywordExtractor.ExtractKeywords(text, request.Keywords);
        var keywordSentiment = _contextSentiment.ExtractKeywordSentiments(text, request.Keywords);

        await _chunkIngestor.IngestChunksAsync(request.FileName, text, new Dictionary<string, object>
        {
            { "Sentiment", sentiment },
            { "SourceType", "PDF" }
        });

        _resultStore.Add(new AnalysisResult(links: new List<LinkedPassage>())
        {
            FileName = request.FileName,
            PageSentimentScore = sentiment,
            KeywordFrequencies = keywords,
            KeywordSentimentScores = keywordSentiment,
            RawSentences = sentences,
            RawText = text
        });

        _logger.LogInformation("Processed {FileName}", request.FileName);
    }

}
