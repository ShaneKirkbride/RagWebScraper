namespace RagWebScraper.Models;

/// <summary>
/// Represents the frequency of an entity within a given time period.
/// </summary>
public record EntityTrend(string Entity, string Period, int Count);
