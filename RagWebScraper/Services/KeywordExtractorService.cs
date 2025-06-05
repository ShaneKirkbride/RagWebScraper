using System.Text.RegularExpressions;

namespace RagWebScraper.Services
{
    public class KeywordExtractorService : IKeywordExtractor
    {
        public Dictionary<string, int> ExtractKeywords(string text, List<string> searchTerms)
        {
            var frequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var term in searchTerms)
            {
                var count = Regex.Matches(text, @"\b" + Regex.Escape(term) + @"\b", RegexOptions.IgnoreCase).Count;
                if (count > 0)
                    frequency[term] = count;
            }
            return frequency;
        }
    }

}
