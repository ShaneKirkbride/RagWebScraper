namespace RagWebScraper.Services;
using RagWebScraper.Models;

/// <summary>
/// Provides named entity recognition functionality.
/// </summary>
public interface INerService
{
    /// <summary>
    /// Extracts named entities from the input text.
    /// </summary>
    /// <param name="text">The raw input text.</param>
    /// <returns>A list of extracted named entities with type and position info.</returns>
    List<NamedEntity> RecognizeEntities(string text);

    /// <summary>
    /// Tokenizes the sentence and returns labels for each token.
    /// </summary>
    /// <param name="sentence">A single sentence.</param>
    /// <returns>A collection of token/label pairs.</returns>
    List<(string Token, string Label)> RecognizeTokensWithLabels(string sentence);
}
