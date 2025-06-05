namespace RagWebScraper.Services
{
    public class KeywordContextSentimentService : IKeywordContextSentimentService
    {
        private readonly ISentimentAnalyzer _sentimentAnalyzer;

        public KeywordContextSentimentService(ISentimentAnalyzer sentimentAnalyzer)
        {
            _sentimentAnalyzer = sentimentAnalyzer;
        }

        public Dictionary<string, float> ExtractKeywordSentiments(string text, IEnumerable<string> keywords)
        {
            var sentences = text.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

            var keywordSentiments = new Dictionary<string, float>();

            foreach (var keyword in keywords)
            {
                var relevantSentences = sentences
                    .Where(s => s.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!relevantSentences.Any())
                {
                    keywordSentiments[keyword] = 0f;
                    continue;
                }

                var avgScore = relevantSentences
                    .Select(s => _sentimentAnalyzer.AnalyzeSentiment(s))
                    .Average();

                keywordSentiments[keyword] = avgScore;
            }

            return keywordSentiments;
        }
    }
}
