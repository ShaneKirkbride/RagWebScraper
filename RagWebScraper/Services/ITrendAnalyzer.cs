using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Analyzes entity trends over time.
/// </summary>
public interface ITrendAnalyzer
{
    IEnumerable<EntityTrend> ComputeTrends(IEnumerable<DocumentAnalysisResult> docs, TimeSpan binSize);
}
