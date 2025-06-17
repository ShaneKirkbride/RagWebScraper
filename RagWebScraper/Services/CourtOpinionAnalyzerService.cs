using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Provides sentiment and keyword analysis for court opinions.
/// </summary>
public sealed class CourtOpinionAnalyzerService : ICourtOpinionAnalyzerService
{
    private readonly ISentimentAnalyzer _sentimentAnalyzer;
    private readonly IKeywordExtractor _keywordExtractor;
    private readonly IKeywordContextSentimentService _contextSentimentService;

    public CourtOpinionAnalyzerService(
        ISentimentAnalyzer sentimentAnalyzer,
        IKeywordExtractor keywordExtractor,
        IKeywordContextSentimentService contextSentimentService)
    {
        _sentimentAnalyzer = sentimentAnalyzer;
        _keywordExtractor = keywordExtractor;
        _contextSentimentService = contextSentimentService;
    }

    /// <inheritdoc />
    public Task<AnalysisResult> AnalyzeOpinionAsync(CourtOpinion opinion, IEnumerable<string> keywords)
    {
        if (opinion == null)
            throw new ArgumentNullException(nameof(opinion));
        if (keywords == null)
            throw new ArgumentNullException(nameof(keywords));

        var text = opinion.PlainText ?? string.Empty;
        var keywordList = keywords.ToList();
        var result = new AnalysisResult(Enumerable.Empty<LinkedPassage>())
        {
            Url = opinion.CaseName,
            PageSentimentScore = _sentimentAnalyzer.AnalyzeSentiment(text),
            KeywordFrequencies = _keywordExtractor.ExtractKeywords(text, keywordList),
            KeywordSentimentScores = _contextSentimentService.ExtractKeywordSentiments(text, keywordList),
            RawText = text,
            RawSentences = SentenceSplitter.Split(text)
        };

        return Task.FromResult(result);
    }
}
