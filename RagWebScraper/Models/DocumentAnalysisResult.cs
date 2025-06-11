namespace RagWebScraper.Models;

/// <summary>
/// Represents a document with its analysis date and extracted entities.
/// </summary>
public record DocumentAnalysisResult(DateTime Date, IEnumerable<Entity> Entities);
