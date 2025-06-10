namespace RagWebScraper.Services;

using RagWebScraper.Models;

/// <summary>
/// Analyzes web pages for sentiment and keywords.
/// </summary>
public interface IPageAnalyzerService
{
    /// <summary>
    /// Analyzes the specified URL.
    /// </summary>
    /// <param name="url">The page URL to analyze.</param>
    /// <param name="keywords">Keywords to search for.</param>
    /// <returns>The analysis result or <c>null</c> if the page could not be processed.</returns>
    Task<AnalysisResult?> AnalyzePageAsync(string url, List<string> keywords);
}
