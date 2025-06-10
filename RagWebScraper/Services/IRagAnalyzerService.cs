using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Performs analysis across multiple documents for RAG workflows.
/// </summary>
public interface IRagAnalyzerService
{
    /// <summary>
    /// Analyzes the provided document set.
    /// </summary>
    /// <param name="set">Collection of documents to analyze.</param>
    /// <returns>The combined analysis result.</returns>
    Task<RagAnalysisResult> AnalyzeAsync(DocumentSet set);
}
