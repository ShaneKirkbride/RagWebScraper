using System.Text.RegularExpressions;

namespace RagWebScraper.Services
{
    /// <summary>
    /// Extracts keyword occurrence counts from a block of text.
    /// </summary>
    public class KeywordExtractorService : IKeywordExtractor
    {
        /// <summary>
        /// Counts the number of occurrences of the provided search terms within the text.
        /// </summary>
        /// <param name="text">The input text to scan.</param>
        /// <param name="searchTerms">The keywords to count.</param>
        /// <returns>A dictionary mapping each keyword to its frequency.</returns>
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
