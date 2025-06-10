namespace RagWebScraper.Services;

/// <summary>
/// Provides text tokenization services.
/// </summary>
public interface ITokenizer
{
    /// <summary>
    /// Encodes text into token identifiers and readable tokens.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <returns>Token ids and token strings.</returns>
    (IReadOnlyList<int> Ids, IReadOnlyList<string> Tokens) Encode(string text);
}
