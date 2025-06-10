namespace RagWebScraper.Services;

/// <summary>
/// Analyzes the sentiment of text passages.
/// </summary>
public interface ISentimentAnalyzer
{
    /// <summary>
    /// Calculates a sentiment score for the given text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A numeric sentiment score.</returns>
    float AnalyzeSentiment(string text);
}
