using Microsoft.ML.Tokenizers;

namespace RagWebScraper.Services;

public class EnglishRobertaTokenizerAdapter : ITokenizer
{
    private readonly EnglishRobertaTokenizer _tokenizer;

    public EnglishRobertaTokenizerAdapter(EnglishRobertaTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    public (IReadOnlyList<int> Ids, IReadOnlyList<string> Tokens) Encode(string text)
    {
        var ids = _tokenizer.EncodeToIds(text).Select(id => (int)id).ToList();
        var tokens = _tokenizer.EncodeToTokens(text, out _).Select(t => t.Value).ToList();
        return (ids, tokens);
    }
}
