using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Computes entity frequency trends over time.
/// </summary>
public class EntityTrendAnalyzer : ITrendAnalyzer
{
    public IEnumerable<EntityTrend> ComputeTrends(IEnumerable<DocumentAnalysisResult> docs, TimeSpan binSize)
    {
        var trends = docs
            .SelectMany(d => d.Entities.Select(e => new { Period = GetPeriod(d.Date, binSize), e.EntityText }))
            .GroupBy(x => (x.Period, x.EntityText.ToLowerInvariant()))
            .Select(g => new EntityTrend(g.First().EntityText, g.Key.Period, g.Count()))
            .OrderBy(t => t.Period)
            .ThenBy(t => t.Entity);

        return trends;
    }

    private static string GetPeriod(DateTime date, TimeSpan binSize)
    {
        if (binSize.TotalDays >= 90)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            return $"{date.Year}-Q{quarter}";
        }

        return date.ToString("yyyy-MM");
    }
}
