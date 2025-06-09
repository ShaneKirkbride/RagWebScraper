namespace RagWebScraper.Services;
using RagWebScraper.Models;

public interface INerService
{
    /// <summary>
    /// Extracts named entities from the input text.
    /// </summary>
    /// <param name="text">The raw input text.</param>
    /// <returns>A list of extracted named entities with type and position info.</returns>
    List<NamedEntity> RecognizeEntities(string text);
    List<(string Token, string Label)> RecognizeTokensWithLabels(string sentence);
}