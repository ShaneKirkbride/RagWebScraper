namespace RagWebScraper.Services;

/// <summary>
/// Calculates sentiment scores for given keywords within text.
/// </summary>
public interface IKeywordContextSentimentService
{
    /// <summary>
    /// Evaluates the sentiment around each keyword.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="keywords">Keywords to locate within the text.</param>
    /// <returns>A mapping of keyword to average sentiment score.</returns>
    Dictionary<string, float> ExtractKeywordSentiments(string text, IEnumerable<string> keywords);
}
