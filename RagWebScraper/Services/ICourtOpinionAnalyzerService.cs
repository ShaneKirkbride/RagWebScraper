namespace RagWebScraper.Services;

using RagWebScraper.Models;

/// <summary>
/// Performs sentiment and keyword analysis on court opinions.
/// </summary>
public interface ICourtOpinionAnalyzerService
{
    /// <summary>
    /// Analyzes the supplied opinion.
    /// </summary>
    /// <param name="opinion">Opinion to analyze.</param>
    /// <param name="keywords">Keywords to extract statistics for.</param>
    /// <returns>The analysis result.</returns>
    Task<AnalysisResult> AnalyzeOpinionAsync(CourtOpinion opinion, IEnumerable<string> keywords);
}
