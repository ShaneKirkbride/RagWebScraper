namespace RagWebScraper.Models;

/// <summary>
/// Represents a named entity extracted from text.
/// </summary>
public record Entity(string EntityType, string EntityText, int StartIndex, int EndIndex);
