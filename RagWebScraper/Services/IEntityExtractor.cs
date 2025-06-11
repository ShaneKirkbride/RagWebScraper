using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Extracts named entities from raw text.
/// </summary>
public interface IEntityExtractor
{
    IEnumerable<Entity> ExtractEntities(string text);
}
