using Microsoft.ML.Tokenizers;

namespace RagWebScraper.Services;

/// <summary>
/// Adapter to expose <see cref="EnglishRobertaTokenizer"/> as the <see cref="ITokenizer"/> interface.
/// </summary>
public class EnglishRobertaTokenizerAdapter : ITokenizer
{
    private readonly EnglishRobertaTokenizer _tokenizer;

    /// <summary>
    /// Creates a new adapter wrapping the specified tokenizer.
    /// </summary>
    /// <param name="tokenizer">The underlying tokenizer instance.</param>
    public EnglishRobertaTokenizerAdapter(EnglishRobertaTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    /// <summary>
    /// Tokenizes the provided text.
    /// </summary>
    /// <param name="text">Text to tokenize.</param>
    /// <returns>The token ids and their corresponding string tokens.</returns>
    public (IReadOnlyList<int> Ids, IReadOnlyList<string> Tokens) Encode(string text)
    {
        var ids = _tokenizer.EncodeToIds(text).Select(id => (int)id).ToList();
        var tokens = _tokenizer.EncodeToTokens(text, out _).Select(t => t.Value).ToList();
        return (ids, tokens);
    }
}
