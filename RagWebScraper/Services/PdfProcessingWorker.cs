using RagWebScraper.Models;
using RagWebScraper.Services;

namespace RagWebScraper.Services
{
    public class PdfProcessingWorker : BackgroundService
    {
        private readonly IPdfProcessingQueue _queue;
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
        {
            _queue = queue;
            _logger = logger;
            _extractor = extractor;
            _sentiment = sentiment;
            _keywordExtractor = keywordExtractor;
            _contextSentiment = contextSentiment;
            _chunkIngestor = chunkIngestor;
            _resultStore = resultStore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var request in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    var text = _extractor.ExtractText(request.FileStream);
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
                        RawText = text
                    });

                    _logger.LogInformation("Processed {FileName}", request.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process {FileName}", request.FileName);
                }
            }
        }
    }
}
