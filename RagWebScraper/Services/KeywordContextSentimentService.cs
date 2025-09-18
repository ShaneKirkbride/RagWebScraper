using System.Text.RegularExpressions;

namespace RagWebScraper.Services
{
    /// <summary>
    /// Determines sentiment for each keyword by averaging the sentiment scores of
    /// sentences containing the keyword.
    /// </summary>
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
                // Use word boundaries to avoid matching substrings (e.g. "art" in "cart")
                var pattern = $"\\b{Regex.Escape(keyword)}\\b";
                var relevantSentences = sentences
                    .Where(s => Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase))
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

