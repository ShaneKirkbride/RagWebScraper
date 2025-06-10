namespace RagWebScraper.Services;

/// <summary>
/// Extracts keyword frequencies from text.
/// </summary>
public interface IKeywordExtractor
{
    /// <summary>
    /// Counts the occurrences of each keyword in the provided text.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <param name="keywords">Keywords to search for.</param>
    /// <returns>A mapping of keyword to occurrence count.</returns>
    Dictionary<string, int> ExtractKeywords(string text, List<string> keywords);
}
